using ExileCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace WheresMyReloadAt;

public class WheresMyReloadAt : BaseSettingsPlugin<WheresMyReloadAtSettings>
{
    public override bool Initialise()
    {
        Thread thread = new Thread(CheckNewProcess);
        thread.Start();

        return true;
    }

    private void CheckNewProcess()
    {
        try
        {
            var logFile = Path.Combine(Directory.GetCurrentDirectory(), "Logs", $"Info{DateTime.Now:yyyyMMdd}.log");
            if (!File.Exists(logFile))
            {
                return;
            }

            var lines = new List<string>();
            using var fileStream = new FileStream(logFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);
            string line;
            while ((line = streamReader.ReadLine()) != null)
            {
                lines.Add(line);
            }

            var lastGameNotFound = lines.FindLast(x => x.Contains("game not found", StringComparison.OrdinalIgnoreCase));
            if (lastGameNotFound == null)
            {
                return;
            }

            if (!DateTime.TryParse(lastGameNotFound[..26], out DateTime lastGameNotFoundTime))
            {
                return;
            }

            if (DateTime.Now.Subtract(lastGameNotFoundTime).TotalMinutes >= 1)
            {
                return;
            }

            var lastGameNotFoundIndex = lines.FindLastIndex(x =>
                x.Contains("game not found", StringComparison.OrdinalIgnoreCase));
            if (lastGameNotFoundIndex == -1 || lastGameNotFoundIndex + 1 >= lines.Count)
            {
                return;
            }

            var nextLine = lines[lastGameNotFoundIndex + 1];
            if (!nextLine.Contains("PerfTimer =-> Load files from memory"))
            {
                return;
            }

            var reloadPluginsIndex = lines.FindIndex(
                lastGameNotFoundIndex + 1,
                x => x.Contains("lets reload plugins", StringComparison.OrdinalIgnoreCase)
            );

            if (reloadPluginsIndex == -1)
            {
                DebugWindow.LogMsg("lets reload plugins");

                var timeoutAt = DateTime.Now.AddSeconds(30);
                while ((GameController == null || GameController.Memory == null)
                       && DateTime.Now < timeoutAt)
                {
                    Thread.Sleep(1000);
                }

                if (GameController?.Memory != null)
                {
                    GameController.Memory.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            DebugWindow.LogError($"Error in CheckNewProcess: {ex.Message}");
        }
    }
}
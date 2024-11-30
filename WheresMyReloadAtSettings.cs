using ExileCore.Shared.Interfaces;
using ExileCore.Shared.Nodes;

namespace WheresMyReloadAt;

public class WheresMyReloadAtSettings : ISettings
{
    public ToggleNode Enable { get; set; } = new ToggleNode(false);
}
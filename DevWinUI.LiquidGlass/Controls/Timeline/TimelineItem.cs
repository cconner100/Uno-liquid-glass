// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
namespace DevWinUI.LiquidGlass;

/// <summary>Severity of a <see cref="TimelineItem"/>; controls its dot color.</summary>
public enum TimelineSeverity
{
    Info,
    Success,
    Warning,
    Error,
}

/// <summary>
/// A single entry of a <see cref="Timeline"/>.
/// </summary>
public partial class TimelineItem
{
    public string? Header { get; set; }

    public string? Description { get; set; }

    public string? Time { get; set; }

    public TimelineSeverity Severity { get; set; } = TimelineSeverity.Info;
}

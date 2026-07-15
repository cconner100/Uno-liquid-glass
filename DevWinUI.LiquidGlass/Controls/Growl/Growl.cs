// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only (separate
// GrowlWindow etc.); this port runs on Uno Platform and renders toasts into a
// caller-provided host panel.
using Microsoft.UI.Xaml.Controls;

namespace DevWinUI.LiquidGlass;

/// <summary>Severity of a growl toast; controls its icon and icon color.</summary>
public enum GrowlSeverity
{
    Success,
    Info,
    Warning,
    Error,
}

/// <summary>
/// Static toast-notification API. Call <see cref="Register"/> with a panel
/// (typically a vertical StackPanel overlaid at the top-right of the page),
/// then <see cref="Success(string, string?)"/> / <see cref="Info(string, string?)"/> /
/// <see cref="Warning(string, string?)"/> / <see cref="Error(string, string?)"/>.
/// All calls are no-ops when no host is registered.
/// </summary>
public static partial class Growl
{
    private static readonly TimeSpan AutoDismissDelay = TimeSpan.FromSeconds(4);

    private static Panel? _host;

    /// <summary>Sets the panel new toasts are added to.</summary>
    public static void Register(Panel host) => _host = host;

    /// <summary>Clears the host if it is the given panel (call on page unload).</summary>
    public static void Unregister(Panel host)
    {
        if (ReferenceEquals(_host, host))
        {
            _host = null;
        }
    }

    public static void Success(string message, string? title = null) => Show(GrowlSeverity.Success, message, title ?? "Success");

    public static void Info(string message, string? title = null) => Show(GrowlSeverity.Info, message, title ?? "Info");

    public static void Warning(string message, string? title = null) => Show(GrowlSeverity.Warning, message, title ?? "Warning");

    public static void Error(string message, string? title = null) => Show(GrowlSeverity.Error, message, title ?? "Error");

    private static void Show(GrowlSeverity severity, string message, string? title)
    {
        var host = _host;
        if (host is null)
        {
            return;
        }

        var item = new GrowlItem
        {
            Severity = severity,
            Message = message,
            Title = title,
        };

        host.Children.Add(item);

        var timer = host.DispatcherQueue.CreateTimer();
        timer.Interval = AutoDismissDelay;
        timer.IsRepeating = false;
        // The item holds the timer reference so it is not garbage collected.
        item.AutoDismissTimer = timer;
        timer.Tick += (sender, _) =>
        {
            sender.Stop();
            item.Dismiss();
        };
        timer.Start();
    }
}

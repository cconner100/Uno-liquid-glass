// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream hosts dialogs in dedicated Win32 windows;
// this port is a simplified cross-platform secondary-window helper for Uno.
using Microsoft.UI.Xaml;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// Opens secondary windows on platforms that support multi-window.
/// </summary>
public static class GlassWindowHelper
{
    /// <summary>
    /// Tries to open a new window with the given title and content.
    /// Returns the window, or null on platforms without multi-window support.
    /// </summary>
    /// <param name="title">The window title.</param>
    /// <param name="content">The element to host as the window content.</param>
    /// <param name="width">Desired width in logical pixels.</param>
    /// <param name="height">Desired height in logical pixels.</param>
    public static Window? TryOpenWindow(string title, UIElement content, int width = 520, int height = 400)
    {
        try
        {
            var window = new Window
            {
                Title = title,
            };
            window.Content = content;
            window.Activate();

            // Best effort sizing. AppWindow.Resize takes physical pixels, and the
            // rasterization scale is only known once the content has a XamlRoot —
            // right after Activate it can still report 1.0 (halving the window on
            // Retina displays), so resize when the content has actually loaded.
            if (content is FrameworkElement element)
            {
                if (element.IsLoaded)
                {
                    Resize(window, element, width, height);
                }
                else
                {
                    element.Loaded += (_, _) => Resize(window, element, width, height);
                }
            }

            return window;
        }
        catch
        {
            // Creating or activating secondary windows is not supported everywhere
            // (e.g. single-window mobile targets); signal that with null.
            return null;
        }
    }

    private static void Resize(Window window, FrameworkElement content, int width, int height)
    {
        try
        {
            var scale = content.XamlRoot?.RasterizationScale ?? 1.0;
            window.AppWindow?.Resize(new Windows.Graphics.SizeInt32
            {
                Width = (int)(width * scale),
                Height = (int)(height * scale),
            });
        }
        catch
        {
            // Resizing is cosmetic; keep the window at its default size.
        }
    }
}

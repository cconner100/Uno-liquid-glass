// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only and hosts the
// message box in its own Win32 window; this port is a cross-platform
// re-implementation on top of ContentDialog so it runs on Uno Platform.
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// The button combinations a <see cref="MessageBox"/> can offer.
/// </summary>
public enum MessageBoxButtons
{
    Ok,
    OkCancel,
    YesNo,
    YesNoCancel,
}

/// <summary>
/// The button the user chose to dismiss a <see cref="MessageBox"/>.
/// </summary>
public enum MessageBoxResult
{
    None,
    Ok,
    Cancel,
    Yes,
    No,
}

/// <summary>
/// The severity glyph shown beside the message.
/// </summary>
public enum MessageBoxIcon
{
    None,
    Information,
    Success,
    Warning,
    Error,
    Question,
}

/// <summary>
/// A DevWinUI-style MessageBox API built on ContentDialog, so it picks up the
/// Liquid Glass dialog styling from the application theme on every platform.
/// </summary>
public static class MessageBox
{
    /// <summary>
    /// Shows a message dialog and returns the button the user chose.
    /// </summary>
    /// <param name="xamlRoot">The XamlRoot to host the dialog in (required).</param>
    /// <param name="message">The message body text.</param>
    /// <param name="title">The dialog title.</param>
    /// <param name="buttons">Which button combination to offer.</param>
    /// <param name="icon">The severity glyph shown beside the message.</param>
    public static async Task<MessageBoxResult> ShowAsync(
        XamlRoot xamlRoot,
        string message,
        string title = "",
        MessageBoxButtons buttons = MessageBoxButtons.Ok,
        MessageBoxIcon icon = MessageBoxIcon.None)
    {
        if (xamlRoot is null)
        {
            throw new ArgumentNullException(nameof(xamlRoot));
        }

        // DefaultButton is deliberately not set: the stock template's
        // *AsDefaultButton visual states force the framework AccentButtonStyle
        // (resolved in framework scope, immune to app-level overrides and to the
        // PrimaryButtonStyle property). The Liquid Glass dialog style already
        // renders the primary button as the prominent capsule.
        var dialog = new ContentDialog
        {
            XamlRoot = xamlRoot,
            Title = title,
            Content = BuildContent(message, icon),
        };

        switch (buttons)
        {
            case MessageBoxButtons.Ok:
                dialog.PrimaryButtonText = "OK";
                break;
            case MessageBoxButtons.OkCancel:
                dialog.PrimaryButtonText = "OK";
                dialog.CloseButtonText = "Cancel";
                break;
            case MessageBoxButtons.YesNo:
                dialog.PrimaryButtonText = "Yes";
                dialog.CloseButtonText = "No";
                break;
            case MessageBoxButtons.YesNoCancel:
                dialog.PrimaryButtonText = "Yes";
                dialog.SecondaryButtonText = "No";
                dialog.CloseButtonText = "Cancel";
                break;
        }

        var result = await dialog.ShowAsync();
        return MapResult(buttons, result);
    }

    private static MessageBoxResult MapResult(MessageBoxButtons buttons, ContentDialogResult result)
    {
        switch (buttons)
        {
            case MessageBoxButtons.Ok:
                // The only button is OK; Esc/light-dismiss reports None.
                return result == ContentDialogResult.Primary ? MessageBoxResult.Ok : MessageBoxResult.None;
            case MessageBoxButtons.OkCancel:
                // Esc/close counts as Cancel, matching classic MessageBox semantics.
                return result == ContentDialogResult.Primary ? MessageBoxResult.Ok : MessageBoxResult.Cancel;
            case MessageBoxButtons.YesNo:
                return result == ContentDialogResult.Primary ? MessageBoxResult.Yes : MessageBoxResult.No;
            case MessageBoxButtons.YesNoCancel:
                return result switch
                {
                    ContentDialogResult.Primary => MessageBoxResult.Yes,
                    ContentDialogResult.Secondary => MessageBoxResult.No,
                    _ => MessageBoxResult.Cancel,
                };
            default:
                return MessageBoxResult.None;
        }
    }

    private static UIElement BuildContent(string message, MessageBoxIcon icon)
    {
        var text = new TextBlock
        {
            Text = message,
            TextWrapping = TextWrapping.Wrap,
            VerticalAlignment = VerticalAlignment.Center,
        };

        if (icon == MessageBoxIcon.None)
        {
            return text;
        }

        var grid = new Grid { ColumnSpacing = 14 };
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var glyphIcon = new FontIcon
        {
            Glyph = GetGlyph(icon),
            FontSize = 28,
            Foreground = GetIconBrush(icon),
            VerticalAlignment = VerticalAlignment.Top,
        };

        Grid.SetColumn(glyphIcon, 0);
        Grid.SetColumn(text, 1);
        grid.Children.Add(glyphIcon);
        grid.Children.Add(text);
        return grid;
    }

    private static string GetGlyph(MessageBoxIcon icon) => icon switch
    {
        MessageBoxIcon.Information => "\uE946",
        MessageBoxIcon.Success => "\uE930",
        MessageBoxIcon.Warning => "\uE7BA",
        MessageBoxIcon.Error => "\uEA39",
        MessageBoxIcon.Question => "\uE9CE",
        _ => string.Empty,
    };

    private static Brush GetIconBrush(MessageBoxIcon icon) => icon switch
    {
        MessageBoxIcon.Information => GetThemeBrush("LiquidGlassTintBrush", 0xFF, 0x0A, 0x84, 0xFF),
        MessageBoxIcon.Question => GetThemeBrush("LiquidGlassTintBrush", 0xFF, 0x0A, 0x84, 0xFF),
        MessageBoxIcon.Success => GetThemeBrush("LiquidGlassSuccessBrush", 0xFF, 0x30, 0xD1, 0x58),
        // The Liquid Glass palette has no dedicated warning brush; use Apple's systemOrange.
        MessageBoxIcon.Warning => new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0xFF, 0x9F, 0x0A)),
        MessageBoxIcon.Error => GetThemeBrush("LiquidGlassDestructiveBrush", 0xFF, 0xFF, 0x45, 0x3A),
        _ => new SolidColorBrush(Windows.UI.Color.FromArgb(0xFF, 0x80, 0x80, 0x80)),
    };

    private static Brush GetThemeBrush(string key, byte a, byte r, byte g, byte b)
    {
        try
        {
            if (Application.Current?.Resources is { } resources
                && resources.TryGetValue(key, out var value)
                && value is Brush brush)
            {
                return brush;
            }
        }
        catch
        {
            // Fall through to the hard-coded fallback color.
        }

        return new SolidColorBrush(Windows.UI.Color.FromArgb(a, r, g, b));
    }
}

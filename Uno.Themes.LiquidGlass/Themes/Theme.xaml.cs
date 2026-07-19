using Microsoft.UI.Xaml;

namespace LiquidGlass.Uno;

/// <summary>
/// Liquid Glass theme for Uno Platform, used the same way as Uno.Themes' MaterialTheme:
/// merge a <see cref="LiquidGlassTheme"/> instance into Application.Resources after
/// XamlControlsResources. Optional override sources allow rebranding without forking
/// the library, mirroring MaterialTheme.ColorOverrideSource / FontOverrideSource.
/// </summary>
public sealed partial class LiquidGlassTheme : ResourceDictionary
{
    public LiquidGlassTheme()
    {
        InitializeComponent();

        // Uno's MessageDialog adapter otherwise applies DefaultContentDialogStyle
        // explicitly (bypassing the implicit ContentDialog style), and mobile
        // targets default to native dialogs that cannot consume XAML resources.
        // Loading this theme opts MessageDialog into the cross-platform XAML path
        // and points it at the same public style used by ContentDialog.
        global::Uno.WinRTFeatureConfiguration.MessageDialog.UseNativeDialog = false;
        global::Uno.WinRTFeatureConfiguration.MessageDialog.StyleOverride = "LiquidGlassMessageDialogStyle";
    }

    /// <summary>
    /// ms-appx URI of a ResourceDictionary overriding the Liquid Glass color palette
    /// (e.g. "ms-appx:///MyApp/Styles/LiquidGlassColorsOverride.xaml").
    /// </summary>
    public string? ColorOverrideSource
    {
        get => _colorOverrideSource;
        set
        {
            _colorOverrideSource = value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(value) });
            }
        }
    }
    private string? _colorOverrideSource;

    /// <summary>
    /// ms-appx URI of a ResourceDictionary overriding the Liquid Glass fonts.
    /// </summary>
    public string? FontOverrideSource
    {
        get => _fontOverrideSource;
        set
        {
            _fontOverrideSource = value;
            if (!string.IsNullOrWhiteSpace(value))
            {
                MergedDictionaries.Add(new ResourceDictionary { Source = new Uri(value) });
            }
        }
    }
    private string? _fontOverrideSource;
}

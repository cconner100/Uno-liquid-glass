using Microsoft.UI.Xaml;

namespace LiquidGlass.UnoToolkit;

/// <summary>
/// Loads the canonical Uno Toolkit resources and layers the Liquid Glass styles
/// over every visual control family.
/// </summary>
public sealed partial class UnoToolkitGlassTheme : ResourceDictionary
{
    public UnoToolkitGlassTheme()
    {
        InitializeComponent();
    }
}

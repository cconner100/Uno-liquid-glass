using Microsoft.UI.Xaml;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// Styles for the DevWinUI-inspired Liquid Glass controls. Merge into
/// Application.Resources after <c>LiquidGlassTheme</c> (the control templates
/// resolve the LiquidGlass* material brushes it defines):
///
///   &lt;lg:LiquidGlassTheme /&gt;
///   &lt;devlg:DevWinUIGlassTheme /&gt;
/// </summary>
public sealed partial class DevWinUIGlassTheme : ResourceDictionary
{
    public DevWinUIGlassTheme()
    {
        InitializeComponent();
    }
}

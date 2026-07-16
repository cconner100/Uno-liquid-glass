using Microsoft.UI.Xaml;

namespace LiquidGlass.CommunityToolkit;

/// <summary>
/// Liquid Glass styles for the Windows Community Toolkit controls. Merge into
/// Application.Resources after <c>LiquidGlassTheme</c> (the styles resolve the
/// LiquidGlass* material brushes it defines):
///
///   &lt;lg:LiquidGlassTheme /&gt;
///   &lt;ctlg:CommunityToolkitGlassTheme /&gt;
/// </summary>
public sealed partial class CommunityToolkitGlassTheme : ResourceDictionary
{
    public CommunityToolkitGlassTheme()
    {
        InitializeComponent();
    }
}

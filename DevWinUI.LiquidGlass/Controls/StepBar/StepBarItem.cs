// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
using Microsoft.UI.Xaml;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// A single step of a <see cref="StepBar"/>. Only the header label is used by
/// this simplified port.
/// </summary>
public partial class StepBarItem : DependencyObject
{
    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(StepBarItem), new PropertyMetadata(null));
}

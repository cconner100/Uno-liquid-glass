// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// Sizes for a <see cref="KeyVisual"/> key cap.
/// </summary>
public enum VisualType
{
    Small,
    Medium,
    Large,
}

/// <summary>
/// Renders its content as a physical keyboard key cap in Liquid Glass.
/// </summary>
public partial class KeyVisual : ContentControl
{
    public VisualType VisualType
    {
        get => (VisualType)GetValue(VisualTypeProperty);
        set => SetValue(VisualTypeProperty, value);
    }

    public static readonly DependencyProperty VisualTypeProperty =
        DependencyProperty.Register(nameof(VisualType), typeof(VisualType), typeof(KeyVisual), new PropertyMetadata(VisualType.Medium, (d, e) => ((KeyVisual)d).ApplyVisualType()));

    public KeyVisual()
    {
        DefaultStyleKey = typeof(KeyVisual);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        ApplyVisualType();
    }

    private void ApplyVisualType()
    {
        switch (VisualType)
        {
            case VisualType.Small:
                MinWidth = 28;
                MinHeight = 28;
                FontSize = 12;
                Padding = new Thickness(8, 2, 8, 2);
                break;
            case VisualType.Large:
                MinWidth = 44;
                MinHeight = 44;
                FontSize = 16;
                Padding = new Thickness(12, 6, 12, 6);
                break;
            default:
                MinWidth = 36;
                MinHeight = 36;
                FontSize = 14;
                Padding = new Thickness(10, 4, 10, 4);
                break;
        }
    }
}

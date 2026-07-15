// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// Displays a keyboard shortcut as a row of <see cref="KeyVisual"/> key caps
/// joined by "+" separators. Keys is a comma-separated list, e.g. "Cmd,Shift,P".
/// </summary>
public partial class Shortcut : Control
{
    private const string PART_Panel = "PART_Panel";

    private StackPanel? _panel;

    public string? Keys
    {
        get => (string?)GetValue(KeysProperty);
        set => SetValue(KeysProperty, value);
    }

    public static readonly DependencyProperty KeysProperty =
        DependencyProperty.Register(nameof(Keys), typeof(string), typeof(Shortcut), new PropertyMetadata(null, (d, e) => ((Shortcut)d).RebuildKeys()));

    public VisualType VisualType
    {
        get => (VisualType)GetValue(VisualTypeProperty);
        set => SetValue(VisualTypeProperty, value);
    }

    public static readonly DependencyProperty VisualTypeProperty =
        DependencyProperty.Register(nameof(VisualType), typeof(VisualType), typeof(Shortcut), new PropertyMetadata(VisualType.Medium, (d, e) => ((Shortcut)d).RebuildKeys()));

    public Shortcut()
    {
        DefaultStyleKey = typeof(Shortcut);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _panel = GetTemplateChild(PART_Panel) as StackPanel;
        RebuildKeys();
    }

    private void RebuildKeys()
    {
        if (_panel is null)
        {
            return;
        }

        _panel.Children.Clear();

        if (string.IsNullOrWhiteSpace(Keys))
        {
            return;
        }

        var tokens = Keys!.Split(',');
        var first = true;

        foreach (var token in tokens)
        {
            var key = token.Trim();
            if (key.Length == 0)
            {
                continue;
            }

            if (!first)
            {
                _panel.Children.Add(new TextBlock
                {
                    Text = "+",
                    VerticalAlignment = VerticalAlignment.Center,
                    Opacity = 0.8,
                });
            }

            _panel.Children.Add(new KeyVisual
            {
                Content = key,
                VisualType = VisualType,
            });

            first = false;
        }
    }
}

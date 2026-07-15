// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform. Simplified: horizontal-only, visuals regenerated in code.
using System.Collections;
using System.Collections.Specialized;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// A horizontal step indicator: numbered circles joined by connector lines.
/// Steps before <see cref="StepIndex"/> render as completed (filled tint circle
/// with a check glyph), the current step as a tint ring, remaining steps as
/// pending. Items may be strings, <see cref="StepBarItem"/>s, or any object
/// (ToString is used for the label).
/// </summary>
public partial class StepBar : Control
{
    private const string PART_Root = "PART_Root";
    private const double CircleSize = 28;
    private const double ConnectorThickness = 1.5;

    private Grid? _root;
    private INotifyCollectionChanged? _observedCollection;

    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(StepBar), new PropertyMetadata(null, (d, e) => ((StepBar)d).OnItemsSourceChanged(e.NewValue)));

    /// <summary>Zero-based index of the current step.</summary>
    public int StepIndex
    {
        get => (int)GetValue(StepIndexProperty);
        set => SetValue(StepIndexProperty, value);
    }

    public static readonly DependencyProperty StepIndexProperty =
        DependencyProperty.Register(nameof(StepIndex), typeof(int), typeof(StepBar), new PropertyMetadata(0, (d, _) => ((StepBar)d).Rebuild()));

    /// <summary>Fill of completed circles, the ring and number of the current step, and completed connectors.</summary>
    public Brush? AccentBrush
    {
        get => (Brush?)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    public static readonly DependencyProperty AccentBrushProperty =
        DependencyProperty.Register(nameof(AccentBrush), typeof(Brush), typeof(StepBar), new PropertyMetadata(null, (d, _) => ((StepBar)d).Rebuild()));

    /// <summary>Foreground of the check glyph inside completed circles.</summary>
    public Brush? OnAccentBrush
    {
        get => (Brush?)GetValue(OnAccentBrushProperty);
        set => SetValue(OnAccentBrushProperty, value);
    }

    public static readonly DependencyProperty OnAccentBrushProperty =
        DependencyProperty.Register(nameof(OnAccentBrush), typeof(Brush), typeof(StepBar), new PropertyMetadata(null, (d, _) => ((StepBar)d).Rebuild()));

    /// <summary>Fill of pending (not yet reached) circles.</summary>
    public Brush? PendingFillBrush
    {
        get => (Brush?)GetValue(PendingFillBrushProperty);
        set => SetValue(PendingFillBrushProperty, value);
    }

    public static readonly DependencyProperty PendingFillBrushProperty =
        DependencyProperty.Register(nameof(PendingFillBrush), typeof(Brush), typeof(StepBar), new PropertyMetadata(null, (d, _) => ((StepBar)d).Rebuild()));

    /// <summary>Foreground of pending step numbers and step labels.</summary>
    public Brush? SecondaryBrush
    {
        get => (Brush?)GetValue(SecondaryBrushProperty);
        set => SetValue(SecondaryBrushProperty, value);
    }

    public static readonly DependencyProperty SecondaryBrushProperty =
        DependencyProperty.Register(nameof(SecondaryBrush), typeof(Brush), typeof(StepBar), new PropertyMetadata(null, (d, _) => ((StepBar)d).Rebuild()));

    /// <summary>Foreground of the current step's label.</summary>
    public Brush? LabelBrush
    {
        get => (Brush?)GetValue(LabelBrushProperty);
        set => SetValue(LabelBrushProperty, value);
    }

    public static readonly DependencyProperty LabelBrushProperty =
        DependencyProperty.Register(nameof(LabelBrush), typeof(Brush), typeof(StepBar), new PropertyMetadata(null, (d, _) => ((StepBar)d).Rebuild()));

    /// <summary>Stroke of connectors that have not been completed yet.</summary>
    public Brush? ConnectorBrush
    {
        get => (Brush?)GetValue(ConnectorBrushProperty);
        set => SetValue(ConnectorBrushProperty, value);
    }

    public static readonly DependencyProperty ConnectorBrushProperty =
        DependencyProperty.Register(nameof(ConnectorBrush), typeof(Brush), typeof(StepBar), new PropertyMetadata(null, (d, _) => ((StepBar)d).Rebuild()));

    public StepBar()
    {
        DefaultStyleKey = typeof(StepBar);

        // A long-lived ItemsSource must not root this control after it leaves
        // the tree: detach the CollectionChanged subscription while unloaded.
        Loaded += (_, _) =>
        {
            if (_observedCollection is null && ItemsSource is INotifyCollectionChanged incc)
            {
                _observedCollection = incc;
                incc.CollectionChanged += OnItemsCollectionChanged;
                Rebuild();
            }
        };
        Unloaded += (_, _) =>
        {
            if (_observedCollection is not null)
            {
                _observedCollection.CollectionChanged -= OnItemsCollectionChanged;
                _observedCollection = null;
            }
        };
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        _root = GetTemplateChild(PART_Root) as Grid;
        Rebuild();
    }

    private void OnItemsSourceChanged(object? newValue)
    {
        if (_observedCollection is not null)
        {
            _observedCollection.CollectionChanged -= OnItemsCollectionChanged;
            _observedCollection = null;
        }

        if (newValue is INotifyCollectionChanged incc)
        {
            _observedCollection = incc;
            incc.CollectionChanged += OnItemsCollectionChanged;
        }

        Rebuild();
    }

    private void OnItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Rebuild();

    private List<object> GetItems()
    {
        var items = new List<object>();
        if (ItemsSource is IEnumerable enumerable and not string)
        {
            foreach (var item in enumerable)
            {
                if (item is not null)
                {
                    items.Add(item);
                }
            }
        }
        return items;
    }

    private static string? GetHeader(object item) =>
        item is StepBarItem stepItem ? stepItem.Header?.ToString() : item.ToString();

    private void Rebuild()
    {
        if (_root is null)
        {
            return;
        }

        _root.Children.Clear();
        _root.ColumnDefinitions.Clear();

        var items = GetItems();
        var count = items.Count;
        if (count == 0)
        {
            return;
        }

        var current = Math.Max(0, Math.Min(StepIndex, count - 1));

        for (var i = 0; i < count; i++)
        {
            _root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var cell = new Grid();
            cell.RowDefinitions.Add(new RowDefinition { Height = new GridLength(CircleSize) });
            cell.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetColumn(cell, i);

            // Top row: [left connector | circle | right connector]
            var top = new Grid();
            top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            top.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            top.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            if (i > 0)
            {
                var leftLine = CreateConnector(completed: i <= current);
                Grid.SetColumn(leftLine, 0);
                top.Children.Add(leftLine);
            }

            if (i < count - 1)
            {
                var rightLine = CreateConnector(completed: i < current);
                Grid.SetColumn(rightLine, 2);
                top.Children.Add(rightLine);
            }

            var circle = CreateCircle(i, current);
            Grid.SetColumn(circle, 1);
            top.Children.Add(circle);

            cell.Children.Add(top);

            var header = GetHeader(items[i]);
            if (!string.IsNullOrEmpty(header))
            {
                var label = new TextBlock
                {
                    Text = header,
                    FontSize = 12,
                    TextWrapping = TextWrapping.Wrap,
                    TextAlignment = TextAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(4, 6, 4, 0),
                    Foreground = i == current ? (LabelBrush ?? SecondaryBrush) : SecondaryBrush,
                };
                if (i == current)
                {
                    label.FontWeight = Microsoft.UI.Text.FontWeights.SemiBold;
                }
                Grid.SetRow(label, 1);
                cell.Children.Add(label);
            }

            _root.Children.Add(cell);
        }
    }

    private Rectangle CreateConnector(bool completed) => new()
    {
        Height = ConnectorThickness,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Fill = completed ? AccentBrush : ConnectorBrush,
        Margin = new Thickness(2, 0, 2, 0),
    };

    private Grid CreateCircle(int index, int current)
    {
        var circle = new Grid { Width = CircleSize, Height = CircleSize };
        var ellipse = new Ellipse { Width = CircleSize, Height = CircleSize };

        if (index < current)
        {
            // Completed: filled accent circle with a check glyph.
            ellipse.Fill = AccentBrush;
            circle.Children.Add(ellipse);
            circle.Children.Add(new FontIcon
            {
                Glyph = "\uE73E",
                FontSize = 12,
                Foreground = OnAccentBrush!,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            });
        }
        else if (index == current)
        {
            // Current: accent ring with accent step number.
            ellipse.Stroke = AccentBrush;
            ellipse.StrokeThickness = 2;
            circle.Children.Add(ellipse);
            circle.Children.Add(CreateNumber(index, AccentBrush));
        }
        else
        {
            // Pending: subtle fill with secondary step number.
            ellipse.Fill = PendingFillBrush;
            circle.Children.Add(ellipse);
            circle.Children.Add(CreateNumber(index, SecondaryBrush));
        }

        return circle;
    }

    private static TextBlock CreateNumber(int index, Brush? foreground) => new()
    {
        Text = (index + 1).ToString(),
        FontSize = 13,
        FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
        Foreground = foreground,
        HorizontalAlignment = HorizontalAlignment.Center,
        VerticalAlignment = VerticalAlignment.Center,
    };
}

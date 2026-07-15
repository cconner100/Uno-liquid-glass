// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform. Simplified: vertical-only, visuals regenerated in code.
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// A vertical timeline: a colored dot and connecting line per entry, with
/// header, description, and time text. Items can be declared in XAML through
/// <see cref="Items"/> or supplied through <see cref="ItemsSource"/> (which
/// takes precedence when set).
/// </summary>
[ContentProperty(Name = nameof(Items))]
public partial class Timeline : Control
{
    private const string PART_Root = "PART_Root";
    private const double DotSize = 10;
    private const double GutterWidth = 12;
    private const double LineThickness = 1.5;

    private StackPanel? _root;
    private INotifyCollectionChanged? _observedCollection;

    /// <summary>Items declared as XAML content.</summary>
    public ObservableCollection<TimelineItem> Items { get; } = new();

    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(Timeline), new PropertyMetadata(null, (d, e) => ((Timeline)d).OnItemsSourceChanged(e.NewValue)));

    /// <summary>Dot color for <see cref="TimelineSeverity.Info"/> items.</summary>
    public Brush? InfoBrush
    {
        get => (Brush?)GetValue(InfoBrushProperty);
        set => SetValue(InfoBrushProperty, value);
    }

    public static readonly DependencyProperty InfoBrushProperty =
        DependencyProperty.Register(nameof(InfoBrush), typeof(Brush), typeof(Timeline), new PropertyMetadata(null, (d, _) => ((Timeline)d).Rebuild()));

    /// <summary>Dot color for <see cref="TimelineSeverity.Success"/> items.</summary>
    public Brush? SuccessBrush
    {
        get => (Brush?)GetValue(SuccessBrushProperty);
        set => SetValue(SuccessBrushProperty, value);
    }

    public static readonly DependencyProperty SuccessBrushProperty =
        DependencyProperty.Register(nameof(SuccessBrush), typeof(Brush), typeof(Timeline), new PropertyMetadata(null, (d, _) => ((Timeline)d).Rebuild()));

    /// <summary>Dot color for <see cref="TimelineSeverity.Warning"/> items.</summary>
    public Brush? WarningBrush
    {
        get => (Brush?)GetValue(WarningBrushProperty);
        set => SetValue(WarningBrushProperty, value);
    }

    public static readonly DependencyProperty WarningBrushProperty =
        DependencyProperty.Register(nameof(WarningBrush), typeof(Brush), typeof(Timeline), new PropertyMetadata(null, (d, _) => ((Timeline)d).Rebuild()));

    /// <summary>Dot color for <see cref="TimelineSeverity.Error"/> items.</summary>
    public Brush? ErrorBrush
    {
        get => (Brush?)GetValue(ErrorBrushProperty);
        set => SetValue(ErrorBrushProperty, value);
    }

    public static readonly DependencyProperty ErrorBrushProperty =
        DependencyProperty.Register(nameof(ErrorBrush), typeof(Brush), typeof(Timeline), new PropertyMetadata(null, (d, _) => ((Timeline)d).Rebuild()));

    /// <summary>Stroke of the line connecting the dots.</summary>
    public Brush? ConnectorBrush
    {
        get => (Brush?)GetValue(ConnectorBrushProperty);
        set => SetValue(ConnectorBrushProperty, value);
    }

    public static readonly DependencyProperty ConnectorBrushProperty =
        DependencyProperty.Register(nameof(ConnectorBrush), typeof(Brush), typeof(Timeline), new PropertyMetadata(null, (d, _) => ((Timeline)d).Rebuild()));

    /// <summary>Foreground of item headers.</summary>
    public Brush? HeaderBrush
    {
        get => (Brush?)GetValue(HeaderBrushProperty);
        set => SetValue(HeaderBrushProperty, value);
    }

    public static readonly DependencyProperty HeaderBrushProperty =
        DependencyProperty.Register(nameof(HeaderBrush), typeof(Brush), typeof(Timeline), new PropertyMetadata(null, (d, _) => ((Timeline)d).Rebuild()));

    /// <summary>Foreground of item descriptions and times.</summary>
    public Brush? SecondaryBrush
    {
        get => (Brush?)GetValue(SecondaryBrushProperty);
        set => SetValue(SecondaryBrushProperty, value);
    }

    public static readonly DependencyProperty SecondaryBrushProperty =
        DependencyProperty.Register(nameof(SecondaryBrush), typeof(Brush), typeof(Timeline), new PropertyMetadata(null, (d, _) => ((Timeline)d).Rebuild()));

    public Timeline()
    {
        DefaultStyleKey = typeof(Timeline);
        // Items is owned by this instance, so this subscription cannot outlive it.
        Items.CollectionChanged += (_, _) => Rebuild();

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
        _root = GetTemplateChild(PART_Root) as StackPanel;
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

    private List<TimelineItem> GetItems()
    {
        var items = new List<TimelineItem>();
        if (ItemsSource is IEnumerable enumerable and not string)
        {
            foreach (var item in enumerable)
            {
                if (item is TimelineItem timelineItem)
                {
                    items.Add(timelineItem);
                }
            }
        }
        else
        {
            items.AddRange(Items);
        }
        return items;
    }

    private Brush? GetDotBrush(TimelineSeverity severity) => severity switch
    {
        TimelineSeverity.Success => SuccessBrush,
        TimelineSeverity.Warning => WarningBrush,
        TimelineSeverity.Error => ErrorBrush,
        _ => InfoBrush,
    };

    private void Rebuild()
    {
        if (_root is null)
        {
            return;
        }

        _root.Children.Clear();

        var items = GetItems();
        for (var i = 0; i < items.Count; i++)
        {
            _root.Children.Add(BuildRow(items[i], isLast: i == items.Count - 1));
        }
    }

    private Grid BuildRow(TimelineItem item, bool isLast)
    {
        var row = new Grid();
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(GutterWidth) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        // Gutter: dot aligned with the header's first line, line running to the next dot.
        var gutter = new Grid();

        if (!isLast)
        {
            gutter.Children.Add(new Rectangle
            {
                Width = LineThickness,
                Fill = ConnectorBrush,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
                Margin = new Thickness(0, DotSize + 8, 0, 2),
            });
        }

        gutter.Children.Add(new Ellipse
        {
            Width = DotSize,
            Height = DotSize,
            Fill = GetDotBrush(item.Severity),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 4, 0, 0),
        });

        row.Children.Add(gutter);

        var content = new StackPanel
        {
            Margin = new Thickness(12, 0, 0, isLast ? 0 : 20),
        };

        if (!string.IsNullOrEmpty(item.Header))
        {
            content.Children.Add(new TextBlock
            {
                Text = item.Header,
                FontSize = 14,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = HeaderBrush,
                TextWrapping = TextWrapping.Wrap,
            });
        }

        if (!string.IsNullOrEmpty(item.Description))
        {
            content.Children.Add(new TextBlock
            {
                Text = item.Description,
                FontSize = 13,
                Foreground = SecondaryBrush,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 2, 0, 0),
            });
        }

        if (!string.IsNullOrEmpty(item.Time))
        {
            content.Children.Add(new TextBlock
            {
                Text = item.Time,
                FontSize = 12,
                Foreground = SecondaryBrush,
                Margin = new Thickness(0, 3, 0, 0),
            });
        }

        Grid.SetColumn(content, 1);
        row.Children.Add(content);

        return row;
    }
}

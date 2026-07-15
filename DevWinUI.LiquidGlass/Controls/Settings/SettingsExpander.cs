// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// An expandable settings card: a <see cref="SettingsCard"/>-style header row with an
/// expand chevron, plus a collapsible clear-glass area hosting <see cref="Items"/>
/// (typically more SettingsCards) separated by hairlines.
/// </summary>
[ContentProperty(Name = nameof(Items))]
public partial class SettingsExpander : Control
{
    private const string PART_HeaderCard = "PART_HeaderCard";
    private const string PART_ItemsContainer = "PART_ItemsContainer";
    private const string PART_ItemsControl = "PART_ItemsControl";

    private const string GlyphChevronDown = "\uE70D";
    private const string GlyphChevronUp = "\uE70E";

    private SettingsCard? _headerCard;
    private FrameworkElement? _itemsContainer;
    private ItemsControl? _itemsControl;
    private FontIcon? _chevronIcon;

    /// <summary>Fires when the expander is opened.</summary>
    public event EventHandler? Expanded;

    /// <summary>Fires when the expander is closed.</summary>
    public event EventHandler? Collapsed;

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(SettingsExpander),
            new PropertyMetadata(null));

    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(SettingsExpander),
            new PropertyMetadata(null));

    public IconElement? HeaderIcon
    {
        get => (IconElement?)GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(IconElement), typeof(SettingsExpander),
            new PropertyMetadata(null));

    /// <summary>Content shown on the right side of the header row, next to the chevron.</summary>
    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(SettingsExpander),
            new PropertyMetadata(null));

    /// <summary>The items (typically <see cref="SettingsCard"/>s) shown in the expandable area.</summary>
    public IList<object> Items
    {
        get => (IList<object>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(IList<object>), typeof(SettingsExpander),
            new PropertyMetadata(null, (d, e) => ((SettingsExpander)d).UpdateItemsSource()));

    /// <summary>Optional items source; takes precedence over <see cref="Items"/> when set.</summary>
    public object? ItemsSource
    {
        get => GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(object), typeof(SettingsExpander),
            new PropertyMetadata(null, (d, e) => ((SettingsExpander)d).UpdateItemsSource()));

    /// <summary>Optional element shown above the items when expanded.</summary>
    public UIElement? ItemsHeader
    {
        get => (UIElement?)GetValue(ItemsHeaderProperty);
        set => SetValue(ItemsHeaderProperty, value);
    }

    public static readonly DependencyProperty ItemsHeaderProperty =
        DependencyProperty.Register(nameof(ItemsHeader), typeof(UIElement), typeof(SettingsExpander),
            new PropertyMetadata(null));

    /// <summary>Optional element shown below the items when expanded.</summary>
    public UIElement? ItemsFooter
    {
        get => (UIElement?)GetValue(ItemsFooterProperty);
        set => SetValue(ItemsFooterProperty, value);
    }

    public static readonly DependencyProperty ItemsFooterProperty =
        DependencyProperty.Register(nameof(ItemsFooter), typeof(UIElement), typeof(SettingsExpander),
            new PropertyMetadata(null));

    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public static readonly DependencyProperty IsExpandedProperty =
        DependencyProperty.Register(nameof(IsExpanded), typeof(bool), typeof(SettingsExpander),
            new PropertyMetadata(false, (d, e) => ((SettingsExpander)d).OnIsExpandedChanged((bool)e.NewValue)));

    public SettingsExpander()
    {
        DefaultStyleKey = typeof(SettingsExpander);
        SetValue(ItemsProperty, new List<object>());
    }

    protected override void OnApplyTemplate()
    {
        if (_headerCard is not null)
        {
            _headerCard.Click -= OnHeaderCardClick;
        }

        base.OnApplyTemplate();

        _headerCard = GetTemplateChild(PART_HeaderCard) as SettingsCard;
        _itemsContainer = GetTemplateChild(PART_ItemsContainer) as FrameworkElement;
        _itemsControl = GetTemplateChild(PART_ItemsControl) as ItemsControl;

        if (_headerCard is not null)
        {
            _chevronIcon = new FontIcon { Glyph = IsExpanded ? GlyphChevronUp : GlyphChevronDown, FontSize = 12 };
            _headerCard.ActionIcon = _chevronIcon;
            _headerCard.Click += OnHeaderCardClick;
        }

        UpdateItemsSource();
        UpdateExpandState();
    }

    private void OnHeaderCardClick(object sender, RoutedEventArgs e)
    {
        IsExpanded = !IsExpanded;
    }

    private void OnIsExpandedChanged(bool newValue)
    {
        UpdateExpandState();

        if (newValue)
        {
            Expanded?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Collapsed?.Invoke(this, EventArgs.Empty);
        }
    }

    private void UpdateExpandState()
    {
        if (_itemsContainer is not null)
        {
            _itemsContainer.Visibility = IsExpanded ? Visibility.Visible : Visibility.Collapsed;
        }

        if (_chevronIcon is not null)
        {
            _chevronIcon.Glyph = IsExpanded ? GlyphChevronUp : GlyphChevronDown;
        }

        if (_headerCard is not null)
        {
            // Keep the header's hover/pressed overlay concentric with the outer glass:
            // square its bottom corners while the items area is showing below it.
            var radius = CornerRadius;
            _headerCard.CornerRadius = IsExpanded
                ? new CornerRadius(radius.TopLeft, radius.TopRight, 0, 0)
                : radius;
        }
    }

    private void UpdateItemsSource()
    {
        if (_itemsControl is not null)
        {
            _itemsControl.ItemsSource = ItemsSource ?? Items;
        }
    }
}

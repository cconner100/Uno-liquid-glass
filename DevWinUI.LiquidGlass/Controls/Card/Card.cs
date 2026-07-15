// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// A Liquid Glass content card with optional icon, header and description above
/// the main content area. Set IsInteractive to get hover/pressed glass states.
/// </summary>
public partial class Card : ContentControl
{
    private const string PART_HeaderGrid = "PART_HeaderGrid";
    private const string PART_IconPresenter = "PART_IconPresenter";
    private const string PART_HeaderPresenter = "PART_HeaderPresenter";
    private const string PART_DescriptionPresenter = "PART_DescriptionPresenter";
    private const string PART_ContentPresenter = "PART_ContentPresenter";

    private FrameworkElement? _headerGrid;
    private FrameworkElement? _iconPresenter;
    private FrameworkElement? _headerPresenter;
    private FrameworkElement? _descriptionPresenter;
    private FrameworkElement? _contentPresenter;

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(Card), new PropertyMetadata(null, OnVisualPropertyChanged));

    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(Card), new PropertyMetadata(null, OnVisualPropertyChanged));

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public static readonly DependencyProperty IconProperty =
        DependencyProperty.Register(nameof(Icon), typeof(object), typeof(Card), new PropertyMetadata(null, OnVisualPropertyChanged));

    public bool IsInteractive
    {
        get => (bool)GetValue(IsInteractiveProperty);
        set => SetValue(IsInteractiveProperty, value);
    }

    public static readonly DependencyProperty IsInteractiveProperty =
        DependencyProperty.Register(nameof(IsInteractive), typeof(bool), typeof(Card), new PropertyMetadata(false, (d, e) =>
        {
            var card = (Card)d;
            if (!(bool)e.NewValue)
            {
                VisualStateManager.GoToState(card, "Normal", true);
            }
        }));

    public Card()
    {
        DefaultStyleKey = typeof(Card);
    }

    private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((Card)d).UpdateVisualState();

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _headerGrid = GetTemplateChild(PART_HeaderGrid) as FrameworkElement;
        _iconPresenter = GetTemplateChild(PART_IconPresenter) as FrameworkElement;
        _headerPresenter = GetTemplateChild(PART_HeaderPresenter) as FrameworkElement;
        _descriptionPresenter = GetTemplateChild(PART_DescriptionPresenter) as FrameworkElement;
        _contentPresenter = GetTemplateChild(PART_ContentPresenter) as FrameworkElement;

        UpdateVisualState();
        VisualStateManager.GoToState(this, "Normal", false);
    }

    private void UpdateVisualState()
    {
        var hasHeaderArea = Header is not null || Description is not null || Icon is not null;

        if (_headerGrid is not null)
        {
            _headerGrid.Visibility = hasHeaderArea ? Visibility.Visible : Visibility.Collapsed;
        }

        if (_iconPresenter is not null)
        {
            _iconPresenter.Visibility = Icon is null ? Visibility.Collapsed : Visibility.Visible;
        }

        if (_headerPresenter is not null)
        {
            _headerPresenter.Visibility = Header is null ? Visibility.Collapsed : Visibility.Visible;
        }

        if (_descriptionPresenter is not null)
        {
            _descriptionPresenter.Visibility = Description is null ? Visibility.Collapsed : Visibility.Visible;
        }

        if (_contentPresenter is not null)
        {
            _contentPresenter.Margin = hasHeaderArea && Content is not null
                ? new Thickness(0, 12, 0, 0)
                : new Thickness(0);
        }
    }

    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent);
        UpdateVisualState();
    }

    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        base.OnPointerEntered(e);
        if (IsInteractive)
        {
            VisualStateManager.GoToState(this, "PointerOver", true);
        }
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);
        if (IsInteractive)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }
    }

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (IsInteractive)
        {
            VisualStateManager.GoToState(this, "Pressed", true);
        }
    }

    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (IsInteractive)
        {
            VisualStateManager.GoToState(this, "PointerOver", true);
        }
    }

    protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (IsInteractive)
        {
            VisualStateManager.GoToState(this, "Normal", true);
        }
    }
}

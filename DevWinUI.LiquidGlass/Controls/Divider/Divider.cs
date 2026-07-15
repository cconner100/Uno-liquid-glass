// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Shapes;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// A separator line with optional inline content, horizontal or vertical.
/// </summary>
[ContentProperty(Name = nameof(Content))]
public partial class Divider : Control
{
    private const string PART_ColumnStart = "PART_ColumnStart";
    private const string PART_ColumnEnd = "PART_ColumnEnd";
    private const string PART_StretchLine = "PART_StretchLine";
    private const string PART_LeftLine = "PART_LeftLine";
    private const string PART_RightLine = "PART_RightLine";
    private const string PART_Content = "PART_Content";

    private ColumnDefinition? _columnStart;
    private ColumnDefinition? _columnEnd;
    private Line? _stretchLine;
    private Line? _leftLine;
    private Line? _rightLine;
    private ContentPresenter? _contentPresenter;

    private long _horizontalContentAlignmentToken;
    private Thickness _oldContentPadding;

    public Thickness ContentPadding
    {
        get => (Thickness)GetValue(ContentPaddingProperty);
        set => SetValue(ContentPaddingProperty, value);
    }

    public static readonly DependencyProperty ContentPaddingProperty =
        DependencyProperty.Register(nameof(ContentPadding), typeof(Thickness), typeof(Divider), new PropertyMetadata(new Thickness(24, 0, 24, 0)));

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public static readonly DependencyProperty ContentProperty =
        DependencyProperty.Register(nameof(Content), typeof(object), typeof(Divider), new PropertyMetadata(null, (d, e) => ((Divider)d).UpdateContent(e.NewValue)));

    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public static readonly DependencyProperty OrientationProperty =
        DependencyProperty.Register(nameof(Orientation), typeof(Orientation), typeof(Divider), new PropertyMetadata(Orientation.Horizontal, (d, e) =>
        {
            var divider = (Divider)d;
            divider.UpdateOrientation((Orientation)e.NewValue);
            divider.UpdateHorizontalContentAlignment();
        }));

    public Brush? LineStroke
    {
        get => (Brush?)GetValue(LineStrokeProperty);
        set => SetValue(LineStrokeProperty, value);
    }

    public static readonly DependencyProperty LineStrokeProperty =
        DependencyProperty.Register(nameof(LineStroke), typeof(Brush), typeof(Divider), new PropertyMetadata(default(Brush)));

    public double LineStrokeThickness
    {
        get => (double)GetValue(LineStrokeThicknessProperty);
        set => SetValue(LineStrokeThicknessProperty, value);
    }

    public static readonly DependencyProperty LineStrokeThicknessProperty =
        DependencyProperty.Register(nameof(LineStrokeThickness), typeof(double), typeof(Divider), new PropertyMetadata(1.0));

    public Divider()
    {
        DefaultStyleKey = typeof(Divider);
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _columnStart = GetTemplateChild(PART_ColumnStart) as ColumnDefinition;
        _columnEnd = GetTemplateChild(PART_ColumnEnd) as ColumnDefinition;
        _stretchLine = GetTemplateChild(PART_StretchLine) as Line;
        _leftLine = GetTemplateChild(PART_LeftLine) as Line;
        _rightLine = GetTemplateChild(PART_RightLine) as Line;
        _contentPresenter = GetTemplateChild(PART_Content) as ContentPresenter;

        UnregisterPropertyChangedCallback(HorizontalContentAlignmentProperty, _horizontalContentAlignmentToken);
        _horizontalContentAlignmentToken = RegisterPropertyChangedCallback(HorizontalContentAlignmentProperty, (_, _) => UpdateHorizontalContentAlignment());

        UpdateOrientation(Orientation);
        UpdateContent(Content);
    }

    private void UpdateHorizontalContentAlignment()
    {
        if (_columnStart is null || _columnEnd is null)
        {
            return;
        }

        if (Orientation == Orientation.Vertical || Content is null || (Content is string value && string.IsNullOrEmpty(value)))
        {
            _columnStart.Width = new GridLength(1, GridUnitType.Star);
            _columnEnd.Width = new GridLength(1, GridUnitType.Star);
            return;
        }

        switch (HorizontalContentAlignment)
        {
            case HorizontalAlignment.Left:
                _columnStart.Width = new GridLength(20, GridUnitType.Pixel);
                _columnEnd.Width = new GridLength(1, GridUnitType.Star);
                break;
            case HorizontalAlignment.Right:
                _columnStart.Width = new GridLength(1, GridUnitType.Star);
                _columnEnd.Width = new GridLength(20, GridUnitType.Pixel);
                break;
            default:
                _columnStart.Width = new GridLength(1, GridUnitType.Star);
                _columnEnd.Width = new GridLength(1, GridUnitType.Star);
                break;
        }
    }

    private void UpdateOrientation(Orientation orientation)
    {
        if (_stretchLine is null || _leftLine is null || _rightLine is null || _contentPresenter is null)
        {
            return;
        }

        var vertical = orientation == Orientation.Vertical;
        _stretchLine.Visibility = vertical ? Visibility.Visible : Visibility.Collapsed;
        _leftLine.Visibility = vertical ? Visibility.Collapsed : Visibility.Visible;
        _rightLine.Visibility = vertical ? Visibility.Collapsed : Visibility.Visible;
        _contentPresenter.Visibility = vertical ? Visibility.Collapsed : Visibility.Visible;
    }

    private void UpdateContent(object? content)
    {
        if (ContentPadding != new Thickness(0))
        {
            _oldContentPadding = ContentPadding;
        }

        ContentPadding = content is null || (content is string s && string.IsNullOrEmpty(s))
            ? new Thickness(0)
            : _oldContentPadding;

        UpdateHorizontalContentAlignment();
    }
}

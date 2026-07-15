// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// A single toast notification created by the static <see cref="Growl"/> API.
/// Renders on the glass surface material with a severity icon, title, message,
/// and close button.
/// </summary>
public partial class GrowlItem : Control
{
    private const string PART_Icon = "PART_Icon";
    private const string PART_Title = "PART_Title";
    private const string PART_CloseButton = "PART_CloseButton";

    private FontIcon? _icon;
    private TextBlock? _title;
    private ButtonBase? _closeButton;
    private TranslateTransform _entranceTransform = new();
    private bool _dismissed;

    /// <summary>Auto-dismiss timer; held here so it is not garbage collected.</summary>
    internal DispatcherQueueTimer? AutoDismissTimer { get; set; }

    public string? Title
    {
        get => (string?)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(GrowlItem), new PropertyMetadata(null, (d, _) => ((GrowlItem)d).UpdateVisuals()));

    public string? Message
    {
        get => (string?)GetValue(MessageProperty);
        set => SetValue(MessageProperty, value);
    }

    public static readonly DependencyProperty MessageProperty =
        DependencyProperty.Register(nameof(Message), typeof(string), typeof(GrowlItem), new PropertyMetadata(null));

    public GrowlSeverity Severity
    {
        get => (GrowlSeverity)GetValue(SeverityProperty);
        set => SetValue(SeverityProperty, value);
    }

    public static readonly DependencyProperty SeverityProperty =
        DependencyProperty.Register(nameof(Severity), typeof(GrowlSeverity), typeof(GrowlItem), new PropertyMetadata(GrowlSeverity.Info, (d, _) => ((GrowlItem)d).UpdateVisuals()));

    public Brush? SuccessBrush
    {
        get => (Brush?)GetValue(SuccessBrushProperty);
        set => SetValue(SuccessBrushProperty, value);
    }

    public static readonly DependencyProperty SuccessBrushProperty =
        DependencyProperty.Register(nameof(SuccessBrush), typeof(Brush), typeof(GrowlItem), new PropertyMetadata(null, (d, _) => ((GrowlItem)d).UpdateVisuals()));

    public Brush? InfoBrush
    {
        get => (Brush?)GetValue(InfoBrushProperty);
        set => SetValue(InfoBrushProperty, value);
    }

    public static readonly DependencyProperty InfoBrushProperty =
        DependencyProperty.Register(nameof(InfoBrush), typeof(Brush), typeof(GrowlItem), new PropertyMetadata(null, (d, _) => ((GrowlItem)d).UpdateVisuals()));

    public Brush? WarningBrush
    {
        get => (Brush?)GetValue(WarningBrushProperty);
        set => SetValue(WarningBrushProperty, value);
    }

    public static readonly DependencyProperty WarningBrushProperty =
        DependencyProperty.Register(nameof(WarningBrush), typeof(Brush), typeof(GrowlItem), new PropertyMetadata(null, (d, _) => ((GrowlItem)d).UpdateVisuals()));

    public Brush? ErrorBrush
    {
        get => (Brush?)GetValue(ErrorBrushProperty);
        set => SetValue(ErrorBrushProperty, value);
    }

    public static readonly DependencyProperty ErrorBrushProperty =
        DependencyProperty.Register(nameof(ErrorBrush), typeof(Brush), typeof(GrowlItem), new PropertyMetadata(null, (d, _) => ((GrowlItem)d).UpdateVisuals()));

    public GrowlItem()
    {
        DefaultStyleKey = typeof(GrowlItem);

        // Entrance: start invisible and slightly above, animate in on load.
        Opacity = 0;
        _entranceTransform.Y = -12;
        RenderTransform = _entranceTransform;
        Loaded += OnLoaded;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        if (_closeButton is not null)
        {
            _closeButton.Click -= OnCloseClick;
        }

        _icon = GetTemplateChild(PART_Icon) as FontIcon;
        _title = GetTemplateChild(PART_Title) as TextBlock;
        _closeButton = GetTemplateChild(PART_CloseButton) as ButtonBase;

        if (_closeButton is not null)
        {
            _closeButton.Click += OnCloseClick;
        }

        UpdateVisuals();
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Dismiss();

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        if (_dismissed)
        {
            return;
        }

        var storyboard = new Storyboard();

        var fadeIn = new DoubleAnimation
        {
            To = 1,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
        };
        Storyboard.SetTarget(fadeIn, this);
        Storyboard.SetTargetProperty(fadeIn, "Opacity");
        storyboard.Children.Add(fadeIn);

        var slideIn = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(250)),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut },
        };
        Storyboard.SetTarget(slideIn, _entranceTransform);
        Storyboard.SetTargetProperty(slideIn, "Y");
        storyboard.Children.Add(slideIn);

        storyboard.Begin();
    }

    private void UpdateVisuals()
    {
        if (_icon is not null)
        {
            (_icon.Glyph, _icon.Foreground) = Severity switch
            {
                GrowlSeverity.Success => ("\uE73E", SuccessBrush!),
                GrowlSeverity.Warning => ("\uE7BA", WarningBrush!),
                GrowlSeverity.Error => ("\uEA39", ErrorBrush!),
                _ => ("\uE946", InfoBrush!),
            };
        }

        if (_title is not null)
        {
            _title.Visibility = string.IsNullOrEmpty(Title) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    /// <summary>Fades the toast out and removes it from its host panel.</summary>
    public void Dismiss()
    {
        if (_dismissed)
        {
            return;
        }
        _dismissed = true;

        AutoDismissTimer?.Stop();
        AutoDismissTimer = null;

        if (Parent is not Panel host)
        {
            return;
        }

        var storyboard = new Storyboard();
        var fadeOut = new DoubleAnimation
        {
            To = 0,
            Duration = new Duration(TimeSpan.FromMilliseconds(180)),
        };
        Storyboard.SetTarget(fadeOut, this);
        Storyboard.SetTargetProperty(fadeOut, "Opacity");
        storyboard.Children.Add(fadeOut);
        storyboard.Completed += (_, _) => host.Children.Remove(this);
        storyboard.Begin();
    }
}

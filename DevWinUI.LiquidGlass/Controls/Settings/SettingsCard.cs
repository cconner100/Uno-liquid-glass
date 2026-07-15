// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// A settings row: header icon + header/description on the left, arbitrary content
/// (e.g. a ToggleSwitch) on the right. Set <see cref="IsClickEnabled"/> to make the
/// whole card behave like a button — it then shows the chevron action icon and
/// raises <see cref="ButtonBase.Click"/>. Can be hosted inside a
/// <see cref="SettingsExpander"/> or a <see cref="SettingsGroup"/>.
/// </summary>
public partial class SettingsCard : ButtonBase
{
    private const string NormalState = "Normal";
    private const string PointerOverState = "PointerOver";
    private const string PressedState = "Pressed";
    private const string DisabledState = "Disabled";

    private const string PART_HeaderPresenter = "PART_HeaderPresenter";
    private const string PART_DescriptionPresenter = "PART_DescriptionPresenter";
    private const string PART_HeaderIconPresenterHolder = "PART_HeaderIconPresenterHolder";
    private const string PART_ActionIconPresenterHolder = "PART_ActionIconPresenterHolder";

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(SettingsCard),
            new PropertyMetadata(null, (d, e) => ((SettingsCard)d).UpdateHeaderVisibility()));

    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(SettingsCard),
            new PropertyMetadata(null, (d, e) => ((SettingsCard)d).UpdateDescriptionVisibility()));

    public IconElement? HeaderIcon
    {
        get => (IconElement?)GetValue(HeaderIconProperty);
        set => SetValue(HeaderIconProperty, value);
    }

    public static readonly DependencyProperty HeaderIconProperty =
        DependencyProperty.Register(nameof(HeaderIcon), typeof(IconElement), typeof(SettingsCard),
            new PropertyMetadata(null, (d, e) => ((SettingsCard)d).UpdateHeaderIconVisibility()));

    /// <summary>Icon shown at the far right when <see cref="IsClickEnabled"/> is true. Defaults to a chevron.</summary>
    public IconElement? ActionIcon
    {
        get => (IconElement?)GetValue(ActionIconProperty);
        set => SetValue(ActionIconProperty, value);
    }

    public static readonly DependencyProperty ActionIconProperty =
        DependencyProperty.Register(nameof(ActionIcon), typeof(IconElement), typeof(SettingsCard),
            new PropertyMetadata(null));

    /// <summary>When true, the card acts like a button: hover/pressed glass states, the action icon, and Click.</summary>
    public bool IsClickEnabled
    {
        get => (bool)GetValue(IsClickEnabledProperty);
        set => SetValue(IsClickEnabledProperty, value);
    }

    public static readonly DependencyProperty IsClickEnabledProperty =
        DependencyProperty.Register(nameof(IsClickEnabled), typeof(bool), typeof(SettingsCard),
            new PropertyMetadata(false, (d, e) => ((SettingsCard)d).OnIsClickEnabledChanged()));

    public bool IsActionIconVisible
    {
        get => (bool)GetValue(IsActionIconVisibleProperty);
        set => SetValue(IsActionIconVisibleProperty, value);
    }

    public static readonly DependencyProperty IsActionIconVisibleProperty =
        DependencyProperty.Register(nameof(IsActionIconVisible), typeof(bool), typeof(SettingsCard),
            new PropertyMetadata(true, (d, e) => ((SettingsCard)d).UpdateActionIconVisibility()));

    public SettingsCard()
    {
        DefaultStyleKey = typeof(SettingsCard);
        ActionIcon = new FontIcon { Glyph = "\uE974" }; // ChevronRightSmall
        IsEnabledChanged += OnIsEnabledChanged;
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        UpdateHeaderVisibility();
        UpdateDescriptionVisibility();
        UpdateHeaderIconVisibility();
        UpdateActionIconVisibility();
        OnIsClickEnabledChanged();

        VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, false);
    }

    private void OnIsClickEnabledChanged()
    {
        IsTabStop = IsClickEnabled;
        UpdateActionIconVisibility();

        if (!IsClickEnabled)
        {
            VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, true);
        }
    }

    private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, true);
    }

    protected override void OnPointerEntered(PointerRoutedEventArgs e)
    {
        base.OnPointerEntered(e);
        VisualStateManager.GoToState(this, IsClickEnabled && IsEnabled ? PointerOverState : NormalState, true);
    }

    protected override void OnPointerExited(PointerRoutedEventArgs e)
    {
        base.OnPointerExited(e);
        VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, true);
    }

    protected override void OnPointerCaptureLost(PointerRoutedEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, true);
    }

    protected override void OnPointerCanceled(PointerRoutedEventArgs e)
    {
        base.OnPointerCanceled(e);
        VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, true);
    }

    protected override void OnPointerPressed(PointerRoutedEventArgs e)
    {
        if (IsClickEnabled)
        {
            base.OnPointerPressed(e);
            VisualStateManager.GoToState(this, PressedState, true);
        }
    }

    protected override void OnPointerReleased(PointerRoutedEventArgs e)
    {
        if (IsClickEnabled)
        {
            base.OnPointerReleased(e);
            VisualStateManager.GoToState(this, IsEnabled ? NormalState : DisabledState, true);
        }
    }

    protected override void OnKeyDown(KeyRoutedEventArgs e)
    {
        if (IsClickEnabled)
        {
            base.OnKeyDown(e);
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                VisualStateManager.GoToState(this, PressedState, true);
            }
        }
    }

    protected override void OnKeyUp(KeyRoutedEventArgs e)
    {
        if (IsClickEnabled)
        {
            base.OnKeyUp(e);
            if (e.Key == Windows.System.VirtualKey.Enter || e.Key == Windows.System.VirtualKey.Space)
            {
                VisualStateManager.GoToState(this, NormalState, true);
            }
        }
    }

    private void UpdateHeaderVisibility()
    {
        if (GetTemplateChild(PART_HeaderPresenter) is FrameworkElement headerPresenter)
        {
            headerPresenter.Visibility = IsNullOrEmptyString(Header) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void UpdateDescriptionVisibility()
    {
        if (GetTemplateChild(PART_DescriptionPresenter) is FrameworkElement descriptionPresenter)
        {
            descriptionPresenter.Visibility = IsNullOrEmptyString(Description) ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void UpdateHeaderIconVisibility()
    {
        if (GetTemplateChild(PART_HeaderIconPresenterHolder) is FrameworkElement headerIconPresenter)
        {
            headerIconPresenter.Visibility = HeaderIcon is null ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    private void UpdateActionIconVisibility()
    {
        if (GetTemplateChild(PART_ActionIconPresenterHolder) is FrameworkElement actionIconPresenter)
        {
            actionIconPresenter.Visibility = IsClickEnabled && IsActionIconVisible && ActionIcon is not null
                ? Visibility.Visible
                : Visibility.Collapsed;
        }
    }

    private static bool IsNullOrEmptyString(object? obj) =>
        obj is null || (obj is string s && s.Length == 0);
}

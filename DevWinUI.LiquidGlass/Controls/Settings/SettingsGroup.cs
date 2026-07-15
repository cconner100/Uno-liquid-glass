// Ported from DevWinUI (https://github.com/ghost1372/DevWinUI, MIT license),
// restyled for Liquid Glass. Upstream targets Windows App SDK only; this port
// runs on Uno Platform.
using System.Collections.Generic;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Markup;

namespace DevWinUI.LiquidGlass;

/// <summary>
/// Groups a set of settings rows (<see cref="SettingsCard"/> /
/// <see cref="SettingsExpander"/>) under an optional header and description.
/// </summary>
[ContentProperty(Name = nameof(Items))]
public partial class SettingsGroup : Control
{
    private const string PART_HeaderPresenter = "PART_HeaderPresenter";
    private const string PART_DescriptionPresenter = "PART_DescriptionPresenter";
    private const string PART_ItemsControl = "PART_ItemsControl";

    private ItemsControl? _itemsControl;

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public static readonly DependencyProperty HeaderProperty =
        DependencyProperty.Register(nameof(Header), typeof(object), typeof(SettingsGroup),
            new PropertyMetadata(null, (d, e) => ((SettingsGroup)d).UpdateHeaderVisibility()));

    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(object), typeof(SettingsGroup),
            new PropertyMetadata(null, (d, e) => ((SettingsGroup)d).UpdateDescriptionVisibility()));

    /// <summary>The settings rows stacked inside the group.</summary>
    public IList<object> Items
    {
        get => (IList<object>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public static readonly DependencyProperty ItemsProperty =
        DependencyProperty.Register(nameof(Items), typeof(IList<object>), typeof(SettingsGroup),
            new PropertyMetadata(null, (d, e) => ((SettingsGroup)d).UpdateItemsSource()));

    public SettingsGroup()
    {
        DefaultStyleKey = typeof(SettingsGroup);
        SetValue(ItemsProperty, new List<object>());
    }

    protected override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        _itemsControl = GetTemplateChild(PART_ItemsControl) as ItemsControl;

        UpdateHeaderVisibility();
        UpdateDescriptionVisibility();
        UpdateItemsSource();
    }

    private void UpdateItemsSource()
    {
        if (_itemsControl is not null)
        {
            _itemsControl.ItemsSource = Items;
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

    private static bool IsNullOrEmptyString(object? obj) =>
        obj is null || (obj is string s && s.Length == 0);
}

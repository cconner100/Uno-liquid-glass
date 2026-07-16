using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace LiquidGlass.Uno;

/// <summary>
/// Glass treatment for NavigationView. Its pane brushes are hardcoded to framework
/// theme resources deep inside the stock template, but the inner SplitView exposes
/// PaneBackground as a property — so instead of forking a ~2000-line template, this
/// attached helper sets the pane to the Liquid Glass surface material and keeps it
/// in sync with light/dark theme changes.
///
///   &lt;NavigationView lg:LiquidGlassNavigation.GlassPane="True"&gt;
/// </summary>
public static class LiquidGlassNavigation
{
    public static readonly DependencyProperty GlassPaneProperty = DependencyProperty.RegisterAttached(
        "GlassPane",
        typeof(bool),
        typeof(LiquidGlassNavigation),
        new PropertyMetadata(false, OnGlassPaneChanged));

    public static void SetGlassPane(DependencyObject element, bool value) => element.SetValue(GlassPaneProperty, value);

    public static bool GetGlassPane(DependencyObject element) => (bool)element.GetValue(GlassPaneProperty);

    private static void OnGlassPaneChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not NavigationView navigationView || e.NewValue is not true)
        {
            return;
        }

        navigationView.Loaded += (_, _) => Apply(navigationView);
        navigationView.ActualThemeChanged += (_, _) => Apply(navigationView);
        if (navigationView.IsLoaded)
        {
            Apply(navigationView);
        }
    }

    private const string PanePanelTag = "LiquidGlassPanePanel";

    private static void Apply(NavigationView navigationView)
    {
        if (FindDescendant<SplitView>(navigationView) is not { } splitView)
        {
            return;
        }

        var theme = navigationView.ActualTheme;

        // Apple's Liquid Glass sidebar is a floating rounded glass panel inset from
        // the window edges, with the content layer extending underneath it through
        // the blur. Make the SplitView pane transparent and float an acrylic panel
        // behind the pane content (Uno's Skia AcrylicBrush provides real backdrop
        // blur; on renderers without it the brush falls back to a translucent tint).
        if (splitView.Pane is Grid paneGrid)
        {
            splitView.PaneBackground = new SolidColorBrush(Microsoft.UI.Colors.Transparent);

            var glass = paneGrid.Children.OfType<Border>().FirstOrDefault(b => PanePanelTag.Equals(b.Tag));
            if (glass is null)
            {
                glass = new Border
                {
                    Tag = PanePanelTag,
                    CornerRadius = new CornerRadius(26),
                    BorderThickness = new Thickness(1),
                    IsHitTestVisible = false,
                };
                Grid.SetRowSpan(glass, Math.Max(1, paneGrid.RowDefinitions.Count));
                Grid.SetColumnSpan(glass, Math.Max(1, paneGrid.ColumnDefinitions.Count));
                paneGrid.Children.Insert(0, glass);
                paneGrid.Margin = new Thickness(10, 10, 4, 10);
            }

            glass.Background = ResolveThemeBrush("LiquidGlassPaneBrush", theme);
            glass.BorderBrush = ResolveThemeBrush("LiquidGlassStrokeBrush", theme);
        }
        else if (ResolveThemeBrush("LiquidGlassSurfaceBrush", theme) is { } surface)
        {
            // Unknown pane structure (template changed): fall back to a glass pane fill.
            splitView.PaneBackground = surface;
        }
    }

    /// <summary>
    /// Resolves a brush from the theme's ThemeDictionaries for an explicit theme,
    /// searching Application.Resources and its merged dictionaries recursively.
    /// </summary>
    internal static Brush? ResolveThemeBrush(string key, ElementTheme theme)
    {
        var themeKey = theme == ElementTheme.Light ? "Light" : "Default";
        return FindInDictionary(Application.Current.Resources, key, themeKey) as Brush;
    }

    private static object? FindInDictionary(ResourceDictionary dictionary, string key, string themeKey)
    {
        if (dictionary.ThemeDictionaries.TryGetValue(themeKey, out var themed)
            && themed is ResourceDictionary themedDictionary
            && themedDictionary.TryGetValue(key, out var value))
        {
            return value;
        }

        foreach (var merged in dictionary.MergedDictionaries)
        {
            if (FindInDictionary(merged, key, themeKey) is { } nested)
            {
                return nested;
            }
        }

        return null;
    }

    private static T? FindDescendant<T>(DependencyObject root) where T : class
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match)
            {
                return match;
            }

            if (FindDescendant<T>(child) is { } nested)
            {
                return nested;
            }
        }

        return null;
    }
}

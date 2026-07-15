using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;

namespace CommunityToolkit.LiquidGlass;

/// <summary>
/// Workaround for an Uno quirk that breaks the WCT SettingsExpander header on
/// runtime theme switches: the header's background is applied by visual-state
/// storyboard keyframes ({ThemeResource SettingsCardBackground}), and those
/// keyframe lookups don't re-resolve when the root element's RequestedTheme
/// changes — the header ends up wearing the opposite theme's brush while
/// plain style setters resolve correctly.
///
/// The helper re-asserts the correct theme's brush on the header grid using a
/// zero-length storyboard (animation precedence, so it out-ranks the stale
/// keyframe value) whenever the expander loads, changes theme, or toggles.
///
///   &lt;controls:SettingsExpander ctlg:SettingsGlass.FixExpanderHeader="True"&gt;
/// </summary>
public static class SettingsGlass
{
    public static readonly DependencyProperty FixExpanderHeaderProperty = DependencyProperty.RegisterAttached(
        "FixExpanderHeader",
        typeof(bool),
        typeof(SettingsGlass),
        new PropertyMetadata(false, OnFixExpanderHeaderChanged));

    public static void SetFixExpanderHeader(DependencyObject element, bool value) => element.SetValue(FixExpanderHeaderProperty, value);

    public static bool GetFixExpanderHeader(DependencyObject element) => (bool)element.GetValue(FixExpanderHeaderProperty);

    // Tracks whether the handlers are attached, so toggling the property (or a
    // style re-applying it) can't stack duplicate subscriptions.
    private static readonly DependencyProperty IsAttachedProperty = DependencyProperty.RegisterAttached(
        "IsAttached", typeof(bool), typeof(SettingsGlass), new PropertyMetadata(false));

    // One reusable storyboard per expander: re-Begin()ing an ever-growing pile of
    // hold-forever storyboards would accumulate in the animation engine.
    private static readonly DependencyProperty FixStoryboardProperty = DependencyProperty.RegisterAttached(
        "FixStoryboard", typeof(Storyboard), typeof(SettingsGlass), new PropertyMetadata(null));

    private static void OnFixExpanderHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not FrameworkElement expander || e.NewValue is not true)
        {
            return;
        }

        if ((bool)expander.GetValue(IsAttachedProperty))
        {
            return;
        }

        expander.SetValue(IsAttachedProperty, true);

        expander.Loaded += (_, _) => ScheduleFix(expander);
        expander.ActualThemeChanged += (_, _) => ScheduleFix(expander);
        if (expander is WinUI.Controls.SettingsExpander settingsExpander)
        {
            settingsExpander.Expanded += (_, _) => ScheduleFix(expander);
            settingsExpander.Collapsed += (_, _) => ScheduleFix(expander);
        }

        if (expander.IsLoaded)
        {
            ScheduleFix(expander);
        }
    }

    private static void ScheduleFix(FrameworkElement expander)
    {
        // Let the expander's own visual-state storyboards run first, then win.
        expander.DispatcherQueue.TryEnqueue(() =>
        {
            var headerGrid = FindDescendant<Grid>(expander, "ToggleButtonGrid");
            var headerButton = FindDescendant<Microsoft.UI.Xaml.Controls.Primitives.ToggleButton>(expander, "ExpanderHeader");
            var brush = ResolveThemeBrush("SettingsCardBackground", expander.ActualTheme);
            if (headerGrid is null || brush is null)
            {
                return;
            }

            // The grid's Background is held at Animations precedence by the header's
            // visual-state storyboard (with a mis-resolved brush), so a local set is
            // not enough — out-rank it with our own zero-length storyboard, and keep
            // the ToggleButton's plain property in sync for the template binding.
            if (headerButton is not null)
            {
                headerButton.Background = brush;
            }

            // Reuse (and restart) a single storyboard per expander; the previous
            // run is stopped so only one hold is ever active.
            if (expander.GetValue(FixStoryboardProperty) is Storyboard existing)
            {
                existing.Stop();
                existing.Children.Clear();
            }
            else
            {
                existing = new Storyboard();
                expander.SetValue(FixStoryboardProperty, existing);
            }

            var frames = new ObjectAnimationUsingKeyFrames();
            frames.KeyFrames.Add(new DiscreteObjectKeyFrame { KeyTime = KeyTime.FromTimeSpan(TimeSpan.Zero), Value = brush });
            Storyboard.SetTarget(frames, headerGrid);
            Storyboard.SetTargetProperty(frames, "Background");
            existing.Children.Add(frames);
            existing.Begin();
        });
    }

    /// <summary>
    /// Finds a theme-scoped brush by walking the app's merged dictionaries and
    /// picking the entry for the given theme explicitly (ThemeResource lookup
    /// can't be trusted here — it's exactly what mis-resolves).
    /// </summary>
    private static Brush? ResolveThemeBrush(string key, ElementTheme theme)
    {
        var themeKey = theme == ElementTheme.Light ? "Light" : "Default";
        return Application.Current?.Resources is { } resources ? FindInDictionary(resources, key, themeKey) : null;
    }

    private static Brush? FindInDictionary(ResourceDictionary dictionary, string key, string themeKey)
    {
        if (dictionary.ThemeDictionaries.TryGetValue(themeKey, out var themed)
            && themed is ResourceDictionary themedDictionary
            && themedDictionary.TryGetValue(key, out var value)
            && value is Brush brush)
        {
            return brush;
        }

        foreach (var merged in dictionary.MergedDictionaries)
        {
            if (FindInDictionary(merged, key, themeKey) is { } found)
            {
                return found;
            }
        }

        return null;
    }

    private static T? FindDescendant<T>(DependencyObject root, string name) where T : FrameworkElement
    {
        var count = VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match && match.Name == name)
            {
                return match;
            }

            if (FindDescendant<T>(child, name) is { } nested)
            {
                return nested;
            }
        }

        return null;
    }
}

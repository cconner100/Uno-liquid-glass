using System.Globalization;
using System.Xml.Linq;

namespace LiquidGlassGallery.Tests;

/// <summary>
/// Validates the Uno.Themes.LiquidGlass project resource dictionaries: structure, light/dark
/// symmetry, Apple palette fidelity, and the Liquid Glass translucency invariants.
/// XAML is validated from source so the tests run on any TFM without a UI dispatcher.
/// </summary>
public class LiquidGlassThemeTests
{
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

    private static string ThemesDir => Path.Combine(FindSolutionRoot(), "Uno.Themes.LiquidGlass", "Themes");

    private static string FindSolutionRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "Uno.Themes.LiquidGlass")))
        {
            dir = dir.Parent;
        }

        return dir?.FullName ?? throw new DirectoryNotFoundException("Could not locate the Uno.Themes.LiquidGlass project root.");
    }

    private static IEnumerable<string> AllThemeXamlFiles =>
        Directory.EnumerateFiles(ThemesDir, "*.xaml", SearchOption.AllDirectories);

    private static XDocument LoadXaml(string relativePath) =>
        XDocument.Load(Path.Combine(ThemesDir, relativePath));

    private static (XElement Light, XElement Dark) LoadThemeDictionaries()
    {
        var colors = LoadXaml("Colors.xaml");
        var dictionaries = colors.Root!
            .Element(Xaml + "ResourceDictionary.ThemeDictionaries")!
            .Elements(Xaml + "ResourceDictionary")
            .ToDictionary(d => (string)d.Attribute(X + "Key")!);

        return (dictionaries["Light"], dictionaries["Default"]);
    }

    private static string KeyOf(XElement element) =>
        (string?)element.Attribute(X + "Key") ?? string.Empty;

    private static byte AlphaOf(string color)
    {
        // #RRGGBB, #AARRGGBB, or a named color (treated as opaque)
        if (!color.StartsWith('#'))
        {
            return color == "Transparent" ? (byte)0 : (byte)255;
        }

        return color.Length == 9
            ? byte.Parse(color.Substring(1, 2), NumberStyles.HexNumber)
            : (byte)255;
    }

    [Test]
    public void All_theme_xaml_files_are_well_formed()
    {
        var files = AllThemeXamlFiles.ToList();
        files.Should().NotBeEmpty();

        foreach (var file in files)
        {
            var act = () => XDocument.Load(file);
            act.Should().NotThrow($"'{Path.GetFileName(file)}' must be valid XAML/XML");
        }
    }

    [Test]
    public void Theme_xaml_merges_every_dictionary_and_all_sources_exist()
    {
        var theme = LoadXaml("Theme.xaml");
        var sources = theme.Root!
            .Element(Xaml + "ResourceDictionary.MergedDictionaries")!
            .Elements(Xaml + "ResourceDictionary")
            .Select(d => (string)d.Attribute("Source")!)
            .ToList();

        // Every merged source resolves to a file on disk
        foreach (var source in sources)
        {
            var relative = source.Replace("ms-appx:///LiquidGlass.Uno/Themes/", string.Empty);
            File.Exists(Path.Combine(ThemesDir, relative))
                .Should().BeTrue($"merged source '{source}' must exist");
        }

        // Every control dictionary on disk is merged
        var mergedFiles = sources.Select(Path.GetFileName).ToHashSet();
        foreach (var file in Directory.EnumerateFiles(Path.Combine(ThemesDir, "Controls"), "*.xaml"))
        {
            mergedFiles.Should().Contain(Path.GetFileName(file),
                $"control dictionary '{Path.GetFileName(file)}' must be merged into Theme.xaml");
        }
    }

    [Test]
    public void Colors_defines_light_and_dark_theme_dictionaries()
    {
        var act = LoadThemeDictionaries;
        act.Should().NotThrow();
    }

    [Test]
    public void Light_and_dark_theme_dictionaries_define_identical_key_sets()
    {
        var (light, dark) = LoadThemeDictionaries();
        var lightKeys = light.Elements().Select(KeyOf).ToHashSet();
        var darkKeys = dark.Elements().Select(KeyOf).ToHashSet();

        lightKeys.Except(darkKeys).Should().BeEmpty("every light resource needs a dark counterpart");
        darkKeys.Except(lightKeys).Should().BeEmpty("every dark resource needs a light counterpart");
    }

    [TestCase("LiquidGlassTintColor")]
    [TestCase("LiquidGlassTintBrush")]
    [TestCase("LiquidGlassOnTintBrush")]
    [TestCase("LiquidGlassLabelBrush")]
    [TestCase("LiquidGlassSecondaryLabelBrush")]
    [TestCase("LiquidGlassFillBrush")]
    [TestCase("LiquidGlassFillPointerOverBrush")]
    [TestCase("LiquidGlassFillPressedBrush")]
    [TestCase("LiquidGlassClearFillBrush")]
    [TestCase("LiquidGlassControlFillBrush")]
    [TestCase("LiquidGlassSurfaceBrush")]
    [TestCase("LiquidGlassStrokeBrush")]
    [TestCase("LiquidGlassSpecularBrush")]
    [TestCase("LiquidGlassProminentFillBrush")]
    [TestCase("LiquidGlassProminentStrokeBrush")]
    [TestCase("LiquidGlassSuccessBrush")]
    [TestCase("LiquidGlassDestructiveBrush")]
    [TestCase("SystemAccentColor")]
    public void Required_glass_resource_exists_in_both_themes(string key)
    {
        var (light, dark) = LoadThemeDictionaries();
        light.Elements().Select(KeyOf).Should().Contain(key);
        dark.Elements().Select(KeyOf).Should().Contain(key);
    }

    [Test]
    public void Glass_materials_are_translucent()
    {
        // Liquid Glass is a translucent material: acrylic fills must blur/tint the
        // content behind them, and even their fallbacks must not be fully opaque.
        var (light, dark) = LoadThemeDictionaries();
        string[] glassKeys = ["LiquidGlassFillBrush", "LiquidGlassClearFillBrush", "LiquidGlassControlFillBrush", "LiquidGlassSurfaceBrush"];

        foreach (var themeDict in new[] { light, dark })
        {
            foreach (var key in glassKeys)
            {
                var brush = themeDict.Elements().Single(e => KeyOf(e) == key);
                brush.Name.LocalName.Should().Be("AcrylicBrush", $"{key} must be a backdrop material");

                double.Parse((string)brush.Attribute("TintOpacity")!, CultureInfo.InvariantCulture)
                    .Should().BeLessThan(1.0, $"{key} tint must be translucent");
                AlphaOf((string)brush.Attribute("FallbackColor")!)
                    .Should().BeLessThan(255, $"{key} fallback must stay translucent without backdrop support");
            }
        }
    }

    [Test]
    public void Specular_highlight_fades_out()
    {
        // The specular wash mimics light hitting the top of the glass: bright at the
        // top edge, fully transparent before the middle-bottom of the control.
        var (light, dark) = LoadThemeDictionaries();

        foreach (var themeDict in new[] { light, dark })
        {
            var specular = themeDict.Elements().Single(e => KeyOf(e) == "LiquidGlassSpecularBrush");
            var stops = specular.Elements(Xaml + "GradientStop")
                .OrderBy(s => double.Parse((string)s.Attribute("Offset")!, CultureInfo.InvariantCulture))
                .Select(s => AlphaOf((string)s.Attribute("Color")!))
                .ToList();

            stops.First().Should().BeGreaterThan(stops.Last(), "specular light fades toward the bottom");
            stops.Last().Should().Be(0, "specular must end fully transparent");
        }
    }

    [Test]
    public void Accent_palette_is_apple_system_blue()
    {
        var (light, dark) = LoadThemeDictionaries();

        ((string)light.Elements().Single(e => KeyOf(e) == "SystemAccentColor")).Should().Be("#007AFF");
        ((string)dark.Elements().Single(e => KeyOf(e) == "SystemAccentColor")).Should().Be("#0A84FF");
    }

    [Test]
    public void Success_color_is_apple_system_green()
    {
        var (light, dark) = LoadThemeDictionaries();

        ((string)light.Elements().Single(e => KeyOf(e) == "LiquidGlassSuccessColor")).Should().Be("#34C759");
        ((string)dark.Elements().Single(e => KeyOf(e) == "LiquidGlassSuccessColor")).Should().Be("#30D158");
    }

    [Test]
    public void Explicit_styles_follow_liquidglass_naming_convention()
    {
        foreach (var file in Directory.EnumerateFiles(Path.Combine(ThemesDir, "Controls"), "*.xaml"))
        {
            var styles = XDocument.Load(file).Root!.Elements(Xaml + "Style");
            foreach (var style in styles)
            {
                var key = KeyOf(style);
                if (key.Length == 0)
                {
                    // Keyless styles are implicit registrations; they must delegate
                    // to a named LiquidGlass style rather than define visuals inline.
                    var basedOn = (string?)style.Attribute("BasedOn");
                    basedOn.Should().NotBeNull(
                        $"implicit styles in {Path.GetFileName(file)} must be BasedOn a named LiquidGlass style");
                    basedOn.Should().Contain("LiquidGlass",
                        $"implicit styles in {Path.GetFileName(file)} must delegate to the public theme API");
                    continue;
                }

                key.Should().StartWith("LiquidGlass",
                    $"style keys in {Path.GetFileName(file)} are part of the public theme API");
            }
        }
    }

    [Test]
    public void Implicit_styles_are_based_on_existing_explicit_styles()
    {
        var explicitKeys = Directory
            .EnumerateFiles(Path.Combine(ThemesDir, "Controls"), "*.xaml")
            .SelectMany(f => XDocument.Load(f).Root!.Elements(Xaml + "Style"))
            .Select(KeyOf)
            .ToHashSet();

        // Keyed styles in Theme.xaml are only allowed for well-known framework style
        // keys the platform resolves by name (e.g. ContentDialog's default button).
        string[] frameworkOverrideKeys = ["AccentButtonStyle"];

        var styles = LoadXaml("Theme.xaml").Root!.Elements(Xaml + "Style").ToList();
        styles.Should().NotBeEmpty("the theme applies Liquid Glass by default");

        foreach (var style in styles)
        {
            var key = (string?)style.Attribute(X + "Key");
            if (key is not null)
            {
                frameworkOverrideKeys.Should().Contain(key,
                    "styles in Theme.xaml are implicit except deliberate framework key overrides");
            }

            var basedOn = (string)style.Attribute("BasedOn")!;
            var referenced = basedOn.Replace("{StaticResource", string.Empty).Replace("}", string.Empty).Trim();
            explicitKeys.Should().Contain(referenced,
                $"style for {(string)style.Attribute("TargetType")!} must reference a defined explicit style");
        }
    }

    [Test]
    public void Static_resource_aliases_resolve_within_their_theme_dictionary()
    {
        var (light, dark) = LoadThemeDictionaries();

        foreach (var themeDict in new[] { light, dark })
        {
            var keys = themeDict.Elements().Select(KeyOf).ToHashSet();
            var aliases = themeDict.Elements(Xaml + "StaticResource");

            foreach (var alias in aliases)
            {
                keys.Should().Contain((string)alias.Attribute("ResourceKey")!,
                    $"alias '{KeyOf(alias)}' must point at a resource in the same theme dictionary");
            }
        }
    }

    [Test]
    public void Buttons_are_capsules_with_apple_touch_target()
    {
        // Liquid Glass buttons are capsules: corner radius = min height / 2,
        // and Apple's minimum touch target is 44pt.
        var buttons = LoadXaml(Path.Combine("Controls", "Button.xaml")).Root!;
        var buttonStyle = buttons.Elements(Xaml + "Style")
            .Single(s => KeyOf(s) == "LiquidGlassButtonStyle");

        string SetterValue(string property) => (string)buttonStyle
            .Elements(Xaml + "Setter")
            .Single(s => (string)s.Attribute("Property")! == property)
            .Attribute("Value")!;

        var minHeight = double.Parse(SetterValue("MinHeight"), CultureInfo.InvariantCulture);
        var cornerRadius = double.Parse(SetterValue("CornerRadius"), CultureInfo.InvariantCulture);

        minHeight.Should().BeGreaterThanOrEqualTo(44, "Apple's minimum touch target is 44pt");
        cornerRadius.Should().Be(minHeight / 2, "a capsule's corner radius is half its height");
    }

    [Test]
    public void LiquidGlassTheme_is_a_resource_dictionary_with_override_hooks()
    {
        // Mirrors Uno.Themes' MaterialTheme surface: a ResourceDictionary entry point
        // with color/font override sources for rebranding without forking.
        var type = typeof(LiquidGlass.Uno.LiquidGlassTheme);

        type.BaseType!.Name.Should().Be("ResourceDictionary");
        type.GetProperty("ColorOverrideSource").Should().NotBeNull();
        type.GetProperty("FontOverrideSource").Should().NotBeNull();
    }
}

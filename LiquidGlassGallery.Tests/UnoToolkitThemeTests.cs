using System.Xml.Linq;

namespace LiquidGlassGallery.Tests;

public class UnoToolkitThemeTests
{
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

    private static readonly string[] VisualControlTargets =
    [
        "utu:Card",
        "utu:CardContentControl",
        "utu:Chip",
        "utu:ChipGroup",
        "utu:Divider",
        "utu:DrawerControl",
        "utu:DrawerFlyoutPresenter",
        "utu:ExtendedSplashScreen",
        "utu:LoadingView",
        "utu:NavigationBar",
        "utu:SafeArea",
        "utu:TabBar",
        "utu:TabBarItem",
        "utu:TabBarSelectionIndicatorPresenter",
        "utu:ZoomContentControl"
    ];

    private static string ProjectRoot => Path.Combine(FindSolutionRoot(), "Uno.Toolkit.LiquidGlass");
    private static string ThemesRoot => Path.Combine(ProjectRoot, "Themes");

    [Test]
    public void Every_toolkit_theme_dictionary_is_well_formed_and_merged()
    {
        var files = Directory.EnumerateFiles(ThemesRoot, "*.xaml", SearchOption.AllDirectories).ToList();
        files.Should().NotBeEmpty();

        foreach (var file in files)
        {
            var load = () => XDocument.Load(file);
            load.Should().NotThrow($"{Path.GetFileName(file)} must be valid XAML/XML");
        }

        var theme = XDocument.Load(Path.Combine(ThemesRoot, "Theme.xaml"));
        var merged = theme.Root!
            .Element(Xaml + "ResourceDictionary.MergedDictionaries")!
            .Elements()
            .ToList();

        merged.First().Name.LocalName.Should().Be("ToolkitResources",
            "canonical Uno Toolkit templates must load before Liquid Glass overrides");

        var sources = merged
            .Attributes("Source")
            .Select(attribute => Path.GetFileName(attribute.Value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var file in files.Where(file => !file.EndsWith("Theme.xaml", StringComparison.OrdinalIgnoreCase)))
        {
            sources.Should().Contain(Path.GetFileName(file), $"{Path.GetFileName(file)} must be merged by Theme.xaml");
        }
    }

    [Test]
    public void Every_visual_uno_toolkit_control_has_a_liquid_glass_style()
    {
        var targets = Directory
            .EnumerateFiles(Path.Combine(ThemesRoot, "Controls"), "*.xaml")
            .SelectMany(file => XDocument.Load(file).Descendants(Xaml + "Style"))
            .Select(style => (string?)style.Attribute("TargetType"))
            .Where(target => target is not null)
            .ToHashSet(StringComparer.Ordinal);

        targets.Should().Contain(VisualControlTargets);
    }

    [Test]
    public void Toolkit_light_and_dark_token_sets_are_symmetric()
    {
        var tokens = XDocument.Load(Path.Combine(ThemesRoot, "Tokens.xaml"));
        var dictionaries = tokens.Root!
            .Element(Xaml + "ResourceDictionary.ThemeDictionaries")!
            .Elements(Xaml + "ResourceDictionary")
            .ToDictionary(dictionary => (string)dictionary.Attribute(X + "Key")!);

        var light = dictionaries["Light"].Elements().Select(KeyOf).ToHashSet();
        var dark = dictionaries["Default"].Elements().Select(KeyOf).ToHashSet();

        light.Should().BeEquivalentTo(dark);
        light.Should().Contain(["ChipBackground", "TabBarItemBackgroundSelected", "NavigationBarBackground", "DrawerControlDrawerBackgroundBrush"]);
    }

    [Test]
    public void Uno_toolkit_package_declares_the_core_theme_and_toolkit_dependencies()
    {
        var project = XDocument.Load(Path.Combine(ProjectRoot, "Uno.Toolkit.LiquidGlass.csproj"));

        project.Descendants("ProjectReference")
            .Select(reference => (string?)reference.Attribute("Include"))
            .Should().Contain(reference => reference != null && reference.EndsWith("Uno.Themes.LiquidGlass.csproj", StringComparison.Ordinal));

        project.Descendants("PackageReference")
            .Select(reference => (string?)reference.Attribute("Include"))
            .Should().Contain("Uno.Toolkit.WinUI");
    }

    [Test]
    public void Uno_toolkit_package_identity_matches_its_public_namespace_contract()
    {
        var project = XDocument.Load(Path.Combine(ProjectRoot, "Uno.Toolkit.LiquidGlass.csproj"));
        var properties = project.Descendants("PropertyGroup").Elements()
            .GroupBy(property => property.Name.LocalName)
            .ToDictionary(group => group.Key, group => group.Last().Value);

        properties["PackageId"].Should().Be("LiquidGlass.UnoToolkit");
        properties["AssemblyName"].Should().Be("LiquidGlass.UnoToolkit");
        properties["RootNamespace"].Should().Be("LiquidGlass.UnoToolkit");

        var theme = XDocument.Load(Path.Combine(ThemesRoot, "Theme.xaml"));
        theme.Root!.Attribute(X + "Class")!.Value
            .Should().Be("LiquidGlass.UnoToolkit.UnoToolkitGlassTheme");

        File.ReadAllText(Path.Combine(ThemesRoot, "Theme.xaml.cs"))
            .Should().Contain("namespace LiquidGlass.UnoToolkit;")
            .And.Contain("class UnoToolkitGlassTheme");
    }

    private static string KeyOf(XElement element) => (string?)element.Attribute(X + "Key") ?? string.Empty;

    private static string FindSolutionRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "LiquidGlassGallery.sln")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName
            ?? throw new DirectoryNotFoundException("Could not locate LiquidGlassGallery.sln.");
    }
}

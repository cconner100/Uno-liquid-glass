using System.Globalization;
using System.Xml.Linq;

namespace LiquidGlassGallery.Tests;

public class CommunityToolkitThemeTests
{
    private static readonly XNamespace Xaml = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

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

    private static XDocument LoadTokenTheme() => XDocument.Load(Path.Combine(
        FindSolutionRoot(),
        "CommunityToolkit.LiquidGlass",
        "Themes",
        "Controls",
        "TokenizingTextBox.xaml"));

    private static string KeyOf(XElement element) => (string?)element.Attribute(X + "Key") ?? string.Empty;

    [Test]
    public void Token_item_uses_capsule_metrics_and_required_template_parts()
    {
        var root = LoadTokenTheme().Root!;
        var style = root.Elements(Xaml + "Style")
            .Single(element => KeyOf(element) == "LiquidGlassTokenizingTextBoxItemStyle");
        var setters = style.Elements(Xaml + "Setter")
            .ToDictionary(element => (string)element.Attribute("Property")!);

        var minHeightResource = root.Elements(X + "Double")
            .Single(element => KeyOf(element) == "LiquidGlassTokenMinHeight");
        double.Parse(minHeightResource.Value, CultureInfo.InvariantCulture).Should().Be(32);

        var cornerRadiusResource = root.Elements(Xaml + "CornerRadius")
            .Single(element => KeyOf(element) == "LiquidGlassTokenCornerRadius");
        cornerRadiusResource.Value.Should().Be("16", "the radius must be half the default chip height");

        setters.Should().ContainKeys("MinHeight", "CornerRadius", "Template");
        var template = setters["Template"].Descendants(Xaml + "ControlTemplate").Single();
        var namedParts = template.Descendants()
            .Select(element => (string?)element.Attribute(X + "Name"))
            .Where(name => name is not null)
            .ToHashSet();

        namedParts.Should().Contain(["PART_RootGrid", "PART_ContentPresenter", "PART_RemoveButton"]);
        template.Descendants(Xaml + "Button")
            .Single(button => (string?)button.Attribute(X + "Name") == "PART_RemoveButton")
            .Attribute("VerticalAlignment")!.Value.Should().Be("Center");
    }

    [Test]
    public void Token_item_preserves_interaction_visual_states()
    {
        var states = LoadTokenTheme().Descendants(Xaml + "VisualState")
            .Select(state => (string?)state.Attribute(X + "Name"))
            .Where(name => name is not null)
            .ToHashSet();

        states.Should().Contain([
            "Normal", "PointerOver", "Pressed", "Selected",
            "PointerOverSelected", "PressedSelected", "Enabled", "Disabled"
        ]);
    }

    [Test]
    public void Token_item_light_and_dark_resources_are_symmetric()
    {
        var dictionaries = LoadTokenTheme().Root!
            .Element(Xaml + "ResourceDictionary.ThemeDictionaries")!
            .Elements(Xaml + "ResourceDictionary")
            .ToDictionary(dictionary => (string)dictionary.Attribute(X + "Key")!);

        dictionaries["Light"].Elements().Select(KeyOf)
            .Should().BeEquivalentTo(dictionaries["Default"].Elements().Select(KeyOf));
    }

    [Test]
    public void TokenizingTextBox_routes_token_containers_to_liquid_glass_style()
    {
        var root = LoadTokenTheme().Root!;
        var selector = root.Elements()
            .Single(element => KeyOf(element) == "LiquidGlassTokenizingTextBoxStyleSelector");

        ((string?)selector.Attribute("TokenStyle")).Should().Contain("LiquidGlassTokenizingTextBoxItemStyle");

        var controlStyle = root.Elements(Xaml + "Style")
            .Single(element => KeyOf(element) == "LiquidGlassTokenizingTextBoxStyle");
        controlStyle.Elements(Xaml + "Setter")
            .Single(setter => (string?)setter.Attribute("Property") == "ItemContainerStyleSelector")
            .Attribute("Value")!.Value.Should().Contain("LiquidGlassTokenizingTextBoxStyleSelector");
    }

    [Test]
    public void Gallery_demonstrates_selected_and_disabled_token_states()
    {
        var galleryPages = Path.Combine(FindSolutionRoot(), "LiquidGlassGallery", "Pages");
        var page = XDocument.Load(Path.Combine(galleryPages, "ToolkitInputPage.xaml"));

        page.Descendants()
            .Single(element => (string?)element.Attribute(X + "Name") == "DisabledTagBox")
            .Attribute("IsEnabled")!.Value.Should().Be("False");
        File.ReadAllText(Path.Combine(galleryPages, "ToolkitInputPage.xaml.cs"))
            .Should().Contain("TagBox.SelectedIndex = 1");
    }
}

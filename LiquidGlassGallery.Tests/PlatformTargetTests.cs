using System.Xml.Linq;

namespace LiquidGlassGallery.Tests;

public class PlatformTargetTests
{
    private static readonly string[] ExpectedTfms =
    [
        "net10.0",
        "net10.0-android",
        "net10.0-ios",
        "net10.0-windows10.0.26100",
        "net10.0-browserwasm",
        "net10.0-desktop"
    ];

    [TestCase("LiquidGlassGallery/LiquidGlassGallery.csproj")]
    [TestCase("Uno.Themes.LiquidGlass/Uno.Themes.LiquidGlass.csproj")]
    [TestCase("CommunityToolkit.LiquidGlass/CommunityToolkit.LiquidGlass.csproj")]
    [TestCase("DevWinUI.LiquidGlass/DevWinUI.LiquidGlass.csproj")]
    [TestCase("Uno.Toolkit.LiquidGlass/Uno.Toolkit.LiquidGlass.csproj")]
    public void Every_product_project_targets_every_supported_uno_tfm(string relativeProjectPath)
    {
        var root = FindSolutionRoot();
        var project = XDocument.Load(Path.Combine(root, relativeProjectPath));
        var actual = project.Descendants("TargetFrameworks").Single().Value
            .Split(';', StringSplitOptions.RemoveEmptyEntries)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var expected in ExpectedTfms)
        {
            actual.Should().Contain(expected, $"{relativeProjectPath} must target {expected}");
        }
    }

    [TestCase("Uno.Themes.LiquidGlass/Uno.Themes.LiquidGlass.csproj", "LiquidGlass.Uno")]
    [TestCase("CommunityToolkit.LiquidGlass/CommunityToolkit.LiquidGlass.csproj", "LiquidGlass.CommunityToolkit")]
    [TestCase("DevWinUI.LiquidGlass/DevWinUI.LiquidGlass.csproj", "DevWinUI.LiquidGlass")]
    [TestCase("Uno.Toolkit.LiquidGlass/Uno.Toolkit.LiquidGlass.csproj", "LiquidGlass.UnoToolkit")]
    public void Every_package_uses_its_nuget_id_for_namespace_and_assembly(
        string relativeProjectPath,
        string expectedIdentity)
    {
        var root = FindSolutionRoot();
        var project = XDocument.Load(Path.Combine(root, relativeProjectPath));
        var properties = project.Descendants("PropertyGroup").Elements()
            .GroupBy(property => property.Name.LocalName)
            .ToDictionary(group => group.Key, group => group.Last().Value);

        properties["PackageId"].Should().Be(expectedIdentity);
        properties["RootNamespace"].Should().Be(expectedIdentity);
        properties["AssemblyName"].Should().Be(expectedIdentity);
    }

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

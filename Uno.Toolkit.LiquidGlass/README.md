# LiquidGlass.UnoToolkit

Liquid Glass styles for the controls in [`Uno.Toolkit.WinUI`](https://github.com/unoplatform/uno.toolkit.ui). The package targets .NET 10 for portable, Android, iOS, WinUI 3, WebAssembly, and Skia desktop applications.

## Install and activate

```xml
<PackageReference Include="LiquidGlass.UnoToolkit" Version="1.0.6" />
```

Load it after the base Liquid Glass theme. `UnoToolkitGlassTheme` initializes the canonical `ToolkitResources` dictionary itself:

```xml
<ResourceDictionary.MergedDictionaries>
  <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
  <LiquidGlassTheme xmlns="using:LiquidGlass.Uno" />
  <UnoToolkitGlassTheme xmlns="using:LiquidGlass.UnoToolkit" />
</ResourceDictionary.MergedDictionaries>
```

The theme covers `Card`, `CardContentControl`, `Chip`, `ChipGroup`, `Divider`, `DrawerControl`, `DrawerFlyoutPresenter`, `ExtendedSplashScreen`, `LoadingView`, `NavigationBar`, `SafeArea`, `TabBar`, `TabBarItem`, and `ZoomContentControl`. `AutoLayout` and `ResponsiveView` are layout-only controls; the content they host receives the application-level Liquid Glass styles.

Explicit style keys are also available when only part of a page should use the theme, including `LiquidGlassToolkitCardStyle`, `LiquidGlassToolkitChipStyle`, `LiquidGlassToolkitDrawerStyle`, and the four directional `LiquidGlass*DrawerFlyoutPresenterStyle` resources.

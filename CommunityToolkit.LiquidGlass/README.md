# LiquidGlass.CommunityToolkit

Liquid Glass resources for Windows Community Toolkit controls used by Uno Platform
applications on Android, iOS/iPadOS, WinUI 3, WebAssembly, Linux, macOS, and Skia Windows.

This package extends `LiquidGlass.Uno` with themed resources for settings
cards and expanders, segmented controls, tokenizing text boxes, range selectors,
color pickers, sizers, headered controls, metadata, and DataGrid.

Merge the core theme first, followed by the toolkit theme:

```xml
<ResourceDictionary.MergedDictionaries>
  <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
  <LiquidGlassTheme xmlns="using:Uno.Themes.LiquidGlass" />
  <CommunityToolkitGlassTheme xmlns="using:CommunityToolkit.LiquidGlass" />
</ResourceDictionary.MergedDictionaries>
```

See the [project repository](https://github.com/cconner100/Uno-liquid-glass) for
the full control inventory, gallery, supported TFMs, and screenshots.

# DevWinUI.LiquidGlass

DevWinUI-inspired controls reimplemented for Uno Platform and restyled with the
Liquid Glass design language for Android, iOS/iPadOS, WinUI 3, WebAssembly,
Linux, macOS, and Skia Windows.

The package includes Card, Divider, SettingsCard, SettingsExpander, SettingsGroup,
PinBox, KeyVisual, Shortcut, StepBar, Timeline, LoadingIndicator, Growl,
MessageBox, GlassWindowHelper, and UniformGrid.

Merge the core theme first, followed by the DevWinUI theme:

```xml
<ResourceDictionary.MergedDictionaries>
  <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
  <LiquidGlassTheme xmlns="using:LiquidGlass.Uno" />
  <DevWinUIGlassTheme xmlns="using:DevWinUI.LiquidGlass" />
</ResourceDictionary.MergedDictionaries>
```

See the [project repository](https://github.com/cconner100/Uno-liquid-glass) for
the full control inventory, gallery, supported TFMs, and screenshots.

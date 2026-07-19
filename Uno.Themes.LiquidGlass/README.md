# LiquidGlass.Uno

Apple's [Liquid Glass](https://developer.apple.com/documentation/technologyoverviews/adopting-liquid-glass)
design language as a reusable Uno Platform theme, structured the same way as
[Uno.Themes](https://github.com/unoplatform/Uno.Themes) (Material/Cupertino): a
`ResourceDictionary` entry point merged into `App.xaml`, per-control resource
dictionaries, explicit `LiquidGlass*` styles, implicit styles applied by default,
and override hooks for rebranding.

> **Breaking change:** The public namespace and assembly changed from
> `Uno.Themes.LiquidGlass` to `LiquidGlass.Uno` so they match the NuGet package
> ID. Update XAML `using:` declarations, C# imports, and assembly-qualified names.

## Getting started

```xml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
      <LiquidGlassTheme xmlns="using:LiquidGlass.Uno" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

Optional overrides (mirroring `MaterialTheme`):

```xml
<LiquidGlassTheme xmlns="using:LiquidGlass.Uno"
                  ColorOverrideSource="ms-appx:///MyApp/Styles/LiquidGlassColorsOverride.xaml"
                  FontOverrideSource="ms-appx:///MyApp/Styles/LiquidGlassFontsOverride.xaml" />
```

## How Liquid Glass is encoded

| Apple rule | Implementation |
| --- | --- |
| Translucent material that blurs content behind it | `AcrylicBrush` glass fills (`LiquidGlassFillBrush`, `LiquidGlassSurfaceBrush`, …) with translucent fallbacks for renderers without backdrop support |
| Regular vs clear material | `LiquidGlassFillBrush` (adaptive) vs `LiquidGlassClearFillBrush` (media-rich content) |
| Specular highlights / rim lighting | `LiquidGlassSpecularBrush` wash + `LiquidGlassStrokeBrush` gradient rim on every glass surface |
| Capsule buttons, 44pt touch targets | Button family: `MinHeight="44"`, `CornerRadius="22"` |
| Concentric corner radii | Card radius 26 − padding 16 = nested radius 10 (`LiquidGlass*CornerRadius` tokens in `Common.xaml`) |
| Monochrome glass; color for the primary action only | Glass is the default; `LiquidGlassProminentButtonStyle` carries the tint |
| Adaptive light/dark appearance | Full `Light`/`Default` theme dictionaries with identical key sets |
| Apple system palette | `#007AFF`/`#0A84FF` tint, `#34C759`/`#30D158` switch green, `#FF3B30`/`#FF453A` destructive, Apple label/separator colors |
| Controls float above the content layer | `LiquidGlassCardBorderStyle` / `LiquidGlassSurfaceBorderStyle` glass containers; flyout/dialog surfaces use the heavier material |

## Style catalog

Implicit styles restyle `Button`, `ToggleButton`, `RepeatButton`, `HyperlinkButton`,
`TextBox`, `PasswordBox`, `ToggleSwitch`, `RadioButton`, `CheckBox`, `Slider`,
`ComboBox`, `ProgressBar`, `DatePicker`, `TimePicker`, `CalendarDatePicker`,
`AutoSuggestBox`, `FlyoutPresenter`, `MenuFlyoutPresenter`, `MenuFlyoutItem`,
`MenuFlyoutSeparator`, `ContentDialog`, `TeachingTip`, `TabViewItem`, and the picker flyout
presenters by default. DatePicker/TimePicker are fully retemplated as
segmented glass fields (their template part names are preserved so flyouts and
culture-based field ordering keep working), and AutoSuggestBox gets a glass inner
field that keeps the query button plus a glass suggestions surface.

Explicit keys:

- `LiquidGlassButtonStyle` — monochrome glass capsule (default)
- `LiquidGlassProminentButtonStyle` — tinted primary action
- `LiquidGlassDestructiveButtonStyle` — destructive label on glass
- `LiquidGlassToggleButtonStyle`, `LiquidGlassRepeatButtonStyle`, `LiquidGlassHyperlinkButtonStyle`
- `LiquidGlassTextBoxStyle`, `LiquidGlassPasswordBoxStyle`
- `LiquidGlassToggleSwitchStyle` — 51×31 Apple switch, system green when on
- `LiquidGlassRadioButtonStyle`, `LiquidGlassCheckBoxStyle`, `LiquidGlassSliderStyle`
- `LiquidGlassComboBoxStyle`, `LiquidGlassProgressBarStyle`
- `LiquidGlassDatePickerStyle`, `LiquidGlassTimePickerStyle` — segmented glass fields
- `LiquidGlassDatePickerFlyoutPresenterStyle`, `LiquidGlassTimePickerFlyoutPresenterStyle` — glass picker-wheel popups
- `LiquidGlassCalendarDatePickerStyle`
- `LiquidGlassAutoSuggestBoxStyle`, `LiquidGlassAutoSuggestBoxTextBoxStyle`, `LiquidGlassQueryButtonStyle`
- `LiquidGlassPickerFieldButtonStyle` — the flat glass field the pickers share
- `LiquidGlassCardBorderStyle`, `LiquidGlassSurfaceBorderStyle` — glass containers

Window-level surfaces:

- `LiquidGlassFlyoutPresenterStyle`, `LiquidGlassMenuFlyoutPresenterStyle` — glass flyout/menu surfaces
- `LiquidGlassMenuFlyoutItemStyle` — macOS-style menu rows (tint highlight, white text on hover)
- `LiquidGlassMenuFlyoutSeparatorStyle`
- `LiquidGlassContentDialogStyle` — glass dialog with capsule command buttons
  (`LiquidGlassDialogButtonStyle` / `LiquidGlassDialogProminentButtonStyle`); the theme also
  overrides the framework's `AccentButtonStyle` key so default actions stay prominent glass;
  loading the theme also routes Uno-backed `MessageDialog` instances through this style
- `LiquidGlassTeachingTipStyle` — glass teaching-tip surface with matching action buttons
- `LiquidGlassMessageDialogStyle` — Uno `MessageDialog` adapter template with glass default-button states
- `LiquidGlassTabViewItemStyle` — tabs as an Apple segmented control (selected tab is a glass pill)
- `LiquidGlassNavigationViewItemStyle` / `LiquidGlassNavigationViewItemPresenterStyle` —
  Apple-style sidebar rows: rounded, no selection indicator bar, frosted glass pill for
  the selected row (the indicator part is kept but transparent so NavigationView's
  selection animation machinery keeps working). Hierarchical items are supported:
  children indent per depth and the chevron flips between open/closed.
- NavigationView: its pane brushes are hardcoded deep in the stock template, so the theme
  ships an attached helper instead of a 2000-line fork:
  `<NavigationView lg:LiquidGlassNavigation.GlassPane="True">` turns the pane into a
  floating rounded glass panel (Apple Tahoe-style sidebar) — inset from the window edges,
  frosty acrylic (`LiquidGlassPaneBrush`), rim light — and tracks light/dark changes.
  The content layer extends underneath it through the blur ("background extension effect").

Design tokens live in `Themes/Common.xaml` (shapes, metrics, typography) and
`Themes/Colors.xaml` (palette + materials, per theme).

## Implementation notes

- Some controls are fully retemplated (buttons, text inputs, switch, radio, slider,
  date/time pickers, AutoSuggestBox) because Uno's stock Fluent templates resolve
  their lightweight styling resources from framework scope, so app-level overrides
  of keys like `TextControlBackground` don't take effect — the same reason
  Uno.Themes ships full templates.
- The DatePicker/TimePicker flyout presenters are retemplated too: the
  looping-selector popups sit on the heavier glass surface with an iOS-style
  rounded selection band (`LiquidGlassDatePickerFlyoutPresenterStyle`,
  `LiquidGlassTimePickerFlyoutPresenterStyle`). The wheels themselves
  (LoopingSelector items) keep their stock, theme-aware rendering.
- Uno's Skia renderer gives `AcrylicBrush` **real backdrop blur** (verified on
  macOS desktop with Uno 6.5) — the glass materials genuinely blur the content
  behind them. Note that `RenderTargetBitmap` renders acrylic as its fallback
  color, so in-app screenshots understate the effect; judge blur on screen or via
  OS-level capture.
- Put glass over a rich content layer. Liquid Glass is defined by what it refracts;
  on a flat background it reads as plain gray.
- Don't stack glass on glass: cards are glass, controls inside cards sit on the
  card's material.

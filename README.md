# Liquid Glass for Uno Platform

![Liquid Glass gallery sample page showing the macOS sidebar, glass cards, and button styles](docs/light_buttons.png)

A reusable **Liquid Glass** theme for Uno Platform, based on Apple's
[Liquid Glass design language](https://developer.apple.com/documentation/technologyoverviews/adopting-liquid-glass).
It follows the same resource-dictionary pattern as
[Uno.Themes](https://github.com/unoplatform/Uno.Themes), and includes a gallery that
demonstrates the complete control set in light and dark appearances.

## Breaking namespace change

> **Breaking change:** Package namespaces and assembly names now match their
> NuGet package IDs. This is a source- and binary-breaking change and must ship
> in a new major version.

Update XAML namespace declarations, C# `using` directives, and any reflection or
assembly-qualified type names as follows:

| Package | Previous namespace and assembly | New namespace and assembly |
| --- | --- | --- |
| `LiquidGlass.Uno` | `Uno.Themes.LiquidGlass` | `LiquidGlass.Uno` |
| `LiquidGlass.CommunityToolkit` | `CommunityToolkit.LiquidGlass` | `LiquidGlass.CommunityToolkit` |
| `DevWinUI.LiquidGlass` | `DevWinUI.LiquidGlass` | Unchanged |
| `LiquidGlass.UnoToolkit` | `Uno.Toolkit.LiquidGlass` | `LiquidGlass.UnoToolkit` |

For example, replace `xmlns:lg="using:Uno.Themes.LiquidGlass"` with
`xmlns:lg="using:LiquidGlass.Uno"`. No control style keys or theme class names
were renamed.

## Supported platforms and target frameworks

The gallery and all four NuGet libraries target every platform supported by the
Uno Platform 6.5 single-project template.

| Platform | TFM | Build host and runtime |
| --- | --- | --- |
| Android | `net10.0-android` | Build on Windows, macOS, or Linux with the Android workload. |
| iOS and iPadOS | `net10.0-ios` | Build on macOS with the iOS workload and full Xcode. |
| Windows / WinUI 3 | `net10.0-windows10.0.26100` | Build and run on Windows using Windows App SDK. |
| Browser / WebAssembly | `net10.0-browserwasm` | Build on Windows, macOS, or Linux with `wasm-tools`; run in a modern browser. |
| Linux, macOS, and Skia Windows | `net10.0-desktop` | Uno Skia Desktop using X11/framebuffer, macOS, or Win32 hosting. |
| Reference and tests | `net10.0` | Platform-neutral compilation used by the test project. |

The projects use **Uno Platform 6.5.36**, the **.NET 10** TFMs above, and the
`SkiaRenderer` Uno feature. CI builds the demo and libraries on Linux, macOS, and
Windows so each platform is validated on a compatible host.

The `net10.0-desktop` TFM is Uno's Linux target; there is no separate
`net10.0-linux` TFM. Likewise, WinUI 3 uses the Windows-qualified TFM above,
while Skia-based Windows applications use `net10.0-desktop`.

| Package / project | Purpose |
| --- | --- |
| `LiquidGlass.Uno` | Reusable Liquid Glass styles for WinUI and Uno controls. |
| `LiquidGlass.CommunityToolkit` | Liquid Glass resources for the Windows Community Toolkit controls used by the gallery. |
| `DevWinUI.LiquidGlass` | Liquid Glass implementations of the DevWinUI-inspired controls used by the gallery. |
| `LiquidGlass.UnoToolkit` | Liquid Glass theme for the visual controls in `Uno.Toolkit.WinUI`. |
| `LiquidGlassGallery` | Sample application for Android, iOS/iPadOS, WinUI 3, WebAssembly, Linux, macOS, and Skia Windows. |
| `LiquidGlassGallery.Tests` | NUnit tests for the theme dictionaries, styles, and design invariants. |

## What the theme changes

The theme supplies matching light and dark resource dictionaries and applies the
styles implicitly, so standard controls change without setting a style on every
instance. The design system uses:

- translucent `AcrylicBrush` materials with renderer-safe fallback colors
- specular top highlights and gradient rim lighting on glass surfaces
- rounded, concentric geometry and 44-point minimum touch targets
- capsule controls and Apple system colors for primary, selected, success, and destructive states
- monochrome glass for normal actions, reserving color for selection and primary actions
- hover, pressed, focused, selected, disabled, light, and dark visual states

### Uno and WinUI controls

| Controls | Liquid Glass treatment |
| --- | --- |
| `Button`, `ToggleButton`, `RepeatButton`, `HyperlinkButton` | Rebuilt as 44-point glass capsules with rim light, hover/press states, and separate prominent and destructive variants. The framework `AccentButtonStyle` maps to the prominent glass button. |
| `TextBox`, `PasswordBox` | Fully retemplated as rounded translucent fields with glass fill, subtle border, placeholder treatment, selection colors, and a tinted focus state. |
| `AutoSuggestBox` | Rebuilt with the glass text field, preserved query button, and a glass suggestions surface. |
| `NumberBox` | Uses the glass input field with integrated capsule spin buttons and a matching popup surface. |
| `CheckBox`, `RadioButton` | Restyled with Apple-like indicators, tint-colored checked states, and theme-aware hover, pressed, and disabled states. |
| `ToggleSwitch` | Rebuilt as a 51-by-31 Apple-style switch with a white thumb and system-green on state. |
| `Slider` | Uses a translucent system-gray track, Apple-tint value track, and round white glass thumb. |
| `ComboBox`, `ComboBoxItem` | Rounded glass field plus a glass dropdown; items receive tint selection and readable hover/selected states. |
| `ListViewItem`, `ListBoxItem` | Rounded selection rows with translucent hover feedback and an Apple-tint selected state. |
| `ProgressBar` | Recolored to a quiet glass track with an Apple-tint progress indicator. |
| `RatingControl` | Applies the Apple tint and theme-aware neutral/disabled rating colors. |
| `DatePicker`, `TimePicker` | Fully retemplated as segmented glass fields while preserving WinUI template-part names and culture-based field ordering. |
| `DatePickerFlyoutPresenter`, `TimePickerFlyoutPresenter` | Rebuilt as heavier glass picker-wheel popups with an iOS-style rounded selection band and glass command buttons. |
| `CalendarDatePicker` | Uses the rounded glass input surface, tint focus treatment, and theme-aware calendar chrome. |
| `DropDownButton`, `SplitButton`, `ToggleSplitButton` | Capsule command surfaces with glass separators, chevrons, primary/secondary regions, and checked-state tint. |
| `MenuBar`, `MenuBarItem` | Clear command chrome at rest with rounded glass hover/open states. |
| `MenuFlyoutPresenter`, `MenuFlyoutItem`, `MenuFlyoutSubItem`, `MenuFlyoutSeparator` | Heavier glass menu surface, macOS-style rows, tint hover/selection, readable selected text, submenu chevrons, and glass separators. |
| `CommandBar`, `CommandBarOverflowPresenter` | Glass command surface and overflow menu with matching rounded geometry and spacing. |
| `AppBarButton`, `AppBarToggleButton`, `AppBarSeparator` | Chromeless toolbar actions that gain rounded glass feedback; toggled actions use the tint and separators use the glass hairline. |
| `FlyoutPresenter` | Gives normal flyouts a rounded, elevated glass surface. |
| `ContentDialog` | Rebuilt as a rounded glass window with capsule command buttons; the default action is prominent glass. |
| `ToolTip` | Compact dark/light glass callout with rounded corners and readable label colors. |
| `InfoBar` | Glass notification surface with semantic accent, rounded rim, and matching close/action buttons. |
| `InfoBadge` | Capsule/circle badge geometry with Apple tint and high-contrast text. |
| `PersonPicture` | Glass-backed avatar treatment with tint-colored initials and presence/badge integration. |
| `TabViewItem` | Tabs become an Apple-style segmented control; the selected tab is a raised glass pill and the close button uses glass feedback. |
| `NavigationViewItem` | Sidebar rows lose the Fluent indicator bar and use a rounded frosted selection pill; hierarchical indentation and animated chevrons are preserved. |
| `NavigationView` pane | The `LiquidGlassNavigation.GlassPane` attached helper turns the pane into an inset floating acrylic sidebar with rim lighting and content extending beneath the blur. |
| `Expander` | Rounded glass header, rotating chevron, and clear-glass expanded content area. |
| `TreeViewItem` | Glass hover/selection rows with preserved depth indentation, expander behavior, and drag/drop states. |
| `BreadcrumbBarItem` | Rounded glass breadcrumb segments, tint interaction states, and matching overflow flyout. |
| `PipsPager` | Translucent inactive pips, a tint selected pip, and glass previous/next buttons. |
| `FlipView` | Keeps the content unobstructed and replaces navigation arrows with floating glass buttons. |
| `SplitView` | Uses clear content with a floating glass pane and glass-aware overlay treatment. |
| `Border` opt-in styles | `LiquidGlassCardBorderStyle` and `LiquidGlassSurfaceBorderStyle` provide reusable card and elevated-surface containers. |

### Windows Community Toolkit controls

| Controls | Liquid Glass treatment |
| --- | --- |
| `SettingsCard`, `SettingsExpander` | Stock templates are reskinned with acrylic card/header fills, gradient rims, rounded content, glass chevrons, and correct runtime light/dark switching. |
| `Segmented`, `SegmentedItem` | iOS-style translucent track and a raised selected glass pill; pivot and detached-button variants use Apple tint appropriately. |
| `TokenizingTextBox` | The entry field inherits the glass text treatment; tokens become translucent chips and selected tokens use Apple tint with white text. |
| `RangeSelector` | Matches the Liquid Glass slider with a gray outer track, tint range band, and two round white thumbs. |
| `ColorPicker` | Keeps the toolkit layout, gives channel sliders white round thumbs, and turns NumberBox popups into glass surfaces. |
| `GridSplitter`, `ContentSizer`, `PropertySizer`, `SizerBase` | Clear glass splitter track with a capsule separator grip and glass hover/pressed feedback. |
| `HeaderedContentControl`, `HeaderedItemsControl` | These layout-only toolkit controls expose no theme resources, so the gallery applies Liquid Glass typography to headers and places their content in glass tiles without replacing the stock templates. |
| `MetadataControl` | Supplies a safe template and secondary-label typography for metadata, separators, wrapping, and links. |
| `DataGrid` | Transparent cells on a glass card, hairline grid separators, quiet headers, translucent row hover, Apple-tint selection, and glass-aware text, combo, and autosuggest editors. |

### DevWinUI-inspired controls

| Controls | Liquid Glass treatment |
| --- | --- |
| `Card` | Acrylic card with specular rim, concentric corners, and hover/pressed overlays. |
| `Divider` | Theme-aware Apple separator with horizontal/vertical layout support and optional label content. |
| `SettingsCard`, `SettingsExpander`, `SettingsGroup` | Glass settings surfaces with clear nested rows, separators, rounded group geometry, and expandable content. |
| `PinBox` | Individual glass digit fields with Apple-tint focus borders and password/PIN behavior. |
| `KeyVisual`, `Shortcut` | Rounded glass keyboard keycaps with a subtle lower edge and compact shortcut layout. |
| `StepBar` | Tint-colored progress markers and connectors with clear current/completed step states. |
| `Timeline` | Glass-aware timeline content with tint markers and separator connectors. |
| `LoadingIndicator` | Animated Apple-tint ring that remains readable in light and dark appearances. |
| `Growl`, `GrowlItem` | Floating rounded glass notification cards with semantic icon/color treatment and a glass close action. |
| `MessageBox`, `GlassWindowHelper` | Glass dialog/window treatment with capsule actions and theme-aware hosting behavior. |
| `UniformGrid` | Responsive equal-cell layout used to present controls consistently in the sample gallery. |

### Uno Toolkit controls

| Controls | Liquid Glass treatment |
| --- | --- |
| `Card`, `CardContentControl` | Translucent cards with specular rims, concentric corners, adaptive typography, and hover/pressed states. |
| `Chip`, `ChipGroup` | Capsule glass chips with tint-selected states, removable actions, and multi-selection support. |
| `Divider` | Theme-aware glass hairline with optional secondary-label text. |
| `DrawerControl`, `DrawerFlyoutPresenter` | Heavy frosted drawer material, rounded exposed corners, rim lighting, dimmed backdrop, and directional flyout styles. |
| `LoadingView`, `ExtendedSplashScreen` | Preserve Toolkit loading transitions while applying tint and glass surface tokens. |
| `NavigationBar` | Frosted navigation surface with Liquid Glass foreground and command styling. |
| `SafeArea` | Inset-aware glass container for mobile, WebAssembly, and desktop layouts. |
| `TabBar`, `TabBarItem` | Floating rounded glass tab bar with tint selection and adaptive interaction states. |
| `ZoomContentControl` | Pan-and-zoom viewport enclosed by a rounded glass surface. |
| `AutoLayout`, `ResponsiveView` | Layout-only controls; their hosted content receives the application-level Liquid Glass styles. |

## Using the theme in your own app

Reference the libraries you need and merge their theme dictionaries in `App.xaml`:

```xml
<Application.Resources>
  <ResourceDictionary>
    <ResourceDictionary.MergedDictionaries>
      <XamlControlsResources xmlns="using:Microsoft.UI.Xaml.Controls" />
      <LiquidGlassTheme xmlns="using:LiquidGlass.Uno" />
      <CommunityToolkitGlassTheme xmlns="using:LiquidGlass.CommunityToolkit" />
      <DevWinUIGlassTheme xmlns="using:DevWinUI.LiquidGlass" />
      <UnoToolkitGlassTheme xmlns="using:LiquidGlass.UnoToolkit" />
    </ResourceDictionary.MergedDictionaries>
  </ResourceDictionary>
</Application.Resources>
```

Standard controls pick up Liquid Glass implicitly. Opt-in variants use explicit styles:

```xml
<Button Content="Buy" Style="{StaticResource LiquidGlassProminentButtonStyle}" />
<Button Content="Delete" Style="{StaticResource LiquidGlassDestructiveButtonStyle}" />
<Border Style="{StaticResource LiquidGlassCardBorderStyle}">…</Border>
```

Rebrand without forking the theme:

```xml
<LiquidGlassTheme xmlns="using:LiquidGlass.Uno"
                  ColorOverrideSource="ms-appx:///MyApp/Styles/LiquidGlassColorsOverride.xaml" />
```

See [Uno.Themes.LiquidGlass/README.md](Uno.Themes.LiquidGlass/README.md) for the
design tokens, explicit style keys, and implementation details.

## NuGet packages

After a release has been published, install the core theme, then add either extension
package when the application uses those controls:

```bash
dotnet add package LiquidGlass.Uno
dotnet add package LiquidGlass.CommunityToolkit
dotnet add package DevWinUI.LiquidGlass
dotnet add package LiquidGlass.UnoToolkit
```

The Community Toolkit and DevWinUI packages declare `LiquidGlass.Uno` as a
dependency, so installing either extension also restores the core theme. Each package
contains assemblies for `net10.0`, `net10.0-android`, `net10.0-ios`,
`net10.0-windows10.0.26100`, `net10.0-browserwasm`, and `net10.0-desktop`, plus
a NuGet README, MIT license metadata, repository/source metadata, and a
portable-symbol `.snupkg`.

### Building NuGet packages

To create packages for every TFM, use the GitHub Actions workflow. Apple targets
must be built on macOS and WinUI 3 must be built on Windows, so a single local
host cannot produce the complete package set. The workflow builds the compatible
assets on macOS and Windows, merges them, and verifies that every `.nupkg`
contains all six framework families.

For a local package build containing the TFMs supported by the current host:

```bash
mkdir -p artifacts/packages

dotnet pack Uno.Themes.LiquidGlass/Uno.Themes.LiquidGlass.csproj \
  --configuration Release --output artifacts/packages
dotnet pack CommunityToolkit.LiquidGlass/CommunityToolkit.LiquidGlass.csproj \
  --configuration Release --output artifacts/packages
dotnet pack DevWinUI.LiquidGlass/DevWinUI.LiquidGlass.csproj \
  --configuration Release --output artifacts/packages
dotnet pack Uno.Toolkit.LiquidGlass/Uno.Toolkit.LiquidGlass.csproj \
  --configuration Release --output artifacts/packages
```

Packing on macOS includes the reference, Android, iOS, WebAssembly, and Desktop
assets. Packing on Windows supplies the WinUI 3 assets used by the merge job.

### Publishing a release

The [Uno builds and NuGet packages workflow](.github/workflows/nuget.yml) builds the
gallery and libraries for every TFM and validates all four packages on pull requests
and pushes to `main`. A manual workflow run accepts a NuGet version, uploads the exact
`.nupkg` and `.snupkg` files as a reviewable GitHub artifact, and then waits for
approval before publishing those same files to NuGet.org.

One-time repository setup:

1. In GitHub, create an environment named `nuget.org`, add the required reviewer or
   reviewers, restrict deployment to `main`, and add an environment variable named
   `NUGET_USER` containing the NuGet.org profile name.
2. In NuGet.org **Trusted Publishing**, add a GitHub policy with repository owner
   `cconner100`, repository `Uno-liquid-glass`, workflow file `nuget.yml`, and
   environment `nuget.org`.
3. In GitHub Actions, run **Uno builds and NuGet packages**, enter the new version,
   review the generated package artifact, and approve the `nuget.org` deployment.
   The publish job obtains a short-lived NuGet credential through GitHub OIDC; no
   long-lived API key is stored in GitHub.

## Building and running the gallery

### Prerequisites

Install the .NET 10 SDK, then install only the workloads needed on the current
development machine:

```bash
# Android
dotnet workload install android

# Browser / WebAssembly
dotnet workload install wasm-tools

# iOS and iPadOS; macOS only
dotnet workload install ios
```

The repository pins Uno Platform through `global.json`. Restore the solution once
after cloning or changing its TFMs:

```bash
dotnet restore LiquidGlassGallery.sln
```

Uno automatically skips TFMs that cannot be restored on the current operating
system. Use an explicit `-f`/`--framework` when building or running so that the
intended platform is unambiguous.

### WebAssembly

Build the gallery and all four referenced libraries:

```bash
dotnet build LiquidGlassGallery/LiquidGlassGallery.csproj \
  --configuration Release --framework net10.0-browserwasm
```

Run the WASM development server:

```bash
dotnet run --project LiquidGlassGallery/LiquidGlassGallery.csproj \
  --framework net10.0-browserwasm
```

The command prints the HTTP and HTTPS URLs assigned by `WasmAppHost`; open either
URL in a browser. A typical local address is `http://127.0.0.1:5000/`, but the
port is selected at launch.

### Android

Build a debug APK:

```bash
dotnet build LiquidGlassGallery/LiquidGlassGallery.csproj \
  --configuration Debug --framework net10.0-android
```

Run it on a connected device or active emulator:

```bash
dotnet build LiquidGlassGallery/LiquidGlassGallery.csproj \
  --configuration Debug --framework net10.0-android --target Run
```

### iOS and iPadOS

Full Xcode must be installed and selected; the standalone Command Line Tools are
not sufficient:

```bash
sudo xcode-select --switch /Applications/Xcode.app/Contents/Developer

dotnet build LiquidGlassGallery/LiquidGlassGallery.csproj \
  --configuration Debug --framework net10.0-ios \
  -p:RuntimeIdentifier=iossimulator-arm64
```

To launch the simulator build, add `--target Run` to that command.

### Linux, macOS, and Skia Windows

Build or run the shared Uno Desktop target:

```bash
dotnet build LiquidGlassGallery/LiquidGlassGallery.csproj \
  --configuration Release --framework net10.0-desktop

dotnet run --project LiquidGlassGallery/LiquidGlassGallery.csproj \
  --framework net10.0-desktop
```

On Linux, the existing desktop host selects X11 or Linux framebuffer support.
The same TFM selects the macOS host on macOS and Win32 on Windows.

### Windows / WinUI 3

Run these commands from Windows with the Windows App SDK build tooling available:

```bash
dotnet build LiquidGlassGallery/LiquidGlassGallery.csproj \
  --configuration Release --framework net10.0-windows10.0.26100

dotnet run --project LiquidGlassGallery/LiquidGlassGallery.csproj \
  --framework net10.0-windows10.0.26100
```

This is the native WinUI 3 head. To build Skia Windows instead, use the Desktop
commands from the preceding section.

### Platform-neutral build

The reference TFM is useful for quick compilation and for the NUnit project:

```bash
dotnet build LiquidGlassGallery/LiquidGlassGallery.csproj \
  --configuration Release --framework net10.0
```

The **Dark appearance** switch changes the whole gallery between the matching light
and dark glass palettes at runtime.

### Screenshot automation

The macOS gallery can walk every sample page in both appearances, save each bitmap,
and exit:

```bash
LG_SCREENSHOT_DIR=/tmp/lg-shots \
  dotnet run --project LiquidGlassGallery/LiquidGlassGallery.csproj -f net10.0-desktop
```

## Tests

```bash
dotnet test LiquidGlassGallery.Tests/LiquidGlassGallery.Tests.csproj
```

The tests parse the theme XAML and verify, among other things:

- light and dark dictionaries define identical key sets
- glass fills use translucent acrylic tints and translucent fallbacks
- specular highlights fade to fully transparent
- buttons keep the 44-point capsule geometry
- the palette matches Apple system colors
- every implicit control style is based on a public `LiquidGlass*` explicit style
- every product project declares all six supported Uno TFMs, including WebAssembly

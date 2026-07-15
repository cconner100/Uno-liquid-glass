using LiquidGlassGallery.Pages;

namespace LiquidGlassGallery;

public sealed partial class MainPage : Page
{
    private static readonly (string Tag, Type PageType)[] _pages =
    [
        ("buttons", typeof(ButtonsPage)),
        ("selection", typeof(SelectionPage)),
        ("text", typeof(TextInputPage)),
        ("collections", typeof(CollectionsPage)),
        ("status", typeof(StatusPage)),
        ("windows", typeof(WindowsPage)),
        ("commanding", typeof(CommandingPage)),
        ("inputsstatus", typeof(InputsStatusPage)),
        ("containers", typeof(ContainersPage)),
        ("devcards", typeof(DevCardsPage)),
        ("devsettings", typeof(DevSettingsPage)),
        ("devinput", typeof(DevInputPage)),
        ("devstatus", typeof(DevStatusPage)),
        ("devdialogs", typeof(DevDialogsPage)),
        ("ctlayouts", typeof(ToolkitLayoutsPage)),
        ("ctsettings", typeof(ToolkitSettingsPage)),
        ("ctinput", typeof(ToolkitInputPage)),
        ("ctdatagrid", typeof(ToolkitDataGridPage)),
    ];

    public MainPage()
    {
        this.InitializeComponent();
        ContentFrame.Navigate(typeof(ButtonsPage));
        Loaded += OnLoaded;
    }

    private void OnNavSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.SelectedItem is NavigationViewItem { Tag: string tag })
        {
            var page = Array.Find(_pages, p => p.Tag == tag).PageType;
            if (page is not null && ContentFrame.CurrentSourcePageType != page)
            {
                ContentFrame.Navigate(page);
            }
        }
    }

    private void OnThemeToggled(object sender, RoutedEventArgs e)
    {
        App.SetRootTheme(ThemeToggle.IsOn ? ElementTheme.Dark : ElementTheme.Light);
    }

    private void OnPaneToggleClick(object sender, RoutedEventArgs e)
    {
        if (Nav.IsPaneOpen)
        {
            // LeftMinimal hides the pane entirely when closed (Left collapses to an icon rail)
            Nav.IsPaneOpen = false;
            Nav.PaneDisplayMode = NavigationViewPaneDisplayMode.LeftMinimal;
        }
        else
        {
            Nav.PaneDisplayMode = NavigationViewPaneDisplayMode.Left;
            Nav.IsPaneOpen = true;
        }
    }

    private void OnPaneClosed(NavigationView sender, object args)
    {
        HideSidebarButton.Visibility = Visibility.Collapsed;
        ShowSidebarButton.Visibility = Visibility.Visible;

        // Keep only enough room for the floating reopen button. The page itself
        // still receives the rest of the width released by the sidebar.
        ContentFrame.Margin = new Thickness(44, 0, 0, 0);
    }

    private void OnPaneOpened(NavigationView sender, object args)
    {
        HideSidebarButton.Visibility = Visibility.Visible;
        ShowSidebarButton.Visibility = Visibility.Collapsed;
        ContentFrame.Margin = new Thickness(0);
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        // Automated capture mode for visual verification: set LG_SCREENSHOT_DIR to a
        // directory and the app walks every gallery page in light and dark theme,
        // saves a bitmap of each, then exits.
        var dir = Environment.GetEnvironmentVariable("LG_SCREENSHOT_DIR");
        if (string.IsNullOrEmpty(dir))
        {
            return;
        }

        try
        {
            Directory.CreateDirectory(dir);
            await Task.Delay(1000);

            // Size the window for capture; AppWindow.Resize takes physical pixels.
            var scale = XamlRoot?.RasterizationScale ?? 1.0;
            App.ActiveWindow?.AppWindow?.Resize(new Windows.Graphics.SizeInt32
            {
                Width = (int)(1240 * scale),
                Height = (int)(860 * scale),
            });
            await Task.Delay(500);

            foreach (var theme in new[] { ElementTheme.Light, ElementTheme.Dark })
            {
                App.SetRootTheme(theme);
                ThemeToggle.IsOn = theme == ElementTheme.Dark;
                await Task.Delay(400);

                foreach (var (tag, pageType) in _pages)
                {
                    ContentFrame.Navigate(pageType);
                    await Task.Delay(800);
                    var prefix = $"{theme.ToString().ToLowerInvariant()}_{tag}";
                    await CaptureAsync(this, Path.Combine(dir, $"{prefix}.bmp"));

                    // Also capture the page's full scrollable content (controls below the fold)
                    if (ContentFrame.Content is Page { Content: ScrollViewer { Content: FrameworkElement full } })
                    {
                        await CaptureAsync(full, Path.Combine(dir, $"{prefix}_full.bmp"));
                    }

                    // Picker flyouts live in the popup layer; open and capture them directly
                    if (tag == "status")
                    {
                        await CapturePickerFlyoutAsync<DatePicker>(Path.Combine(dir, $"{prefix}_datepicker_flyout.bmp"));
                        await CapturePickerFlyoutAsync<TimePicker>(Path.Combine(dir, $"{prefix}_timepicker_flyout.bmp"));
                    }

                    if (theme == ElementTheme.Light && tag == "buttons")
                    {
                        OnPaneToggleClick(this, null!);
                        await Task.Delay(600);
                        await CaptureAsync(this, Path.Combine(dir, $"{prefix}_paneclosed.bmp"));
                        OnPaneToggleClick(this, null!);
                        await Task.Delay(500);
                    }

                    if (tag == "windows")
                    {
                        await CaptureMenuFlyoutAsync(Path.Combine(dir, $"{prefix}_menuflyout.bmp"));
                        await CaptureDialogAsync(Path.Combine(dir, $"{prefix}_dialog.bmp"));
                    }

                    if (tag == "ctdatagrid" && FindDescendant<CommunityToolkit.WinUI.UI.Controls.DataGrid>(ContentFrame) is { } dataGrid)
                    {
                        dataGrid.SelectedIndex = 3;
                        await Task.Delay(400);
                        await CaptureAsync(this, Path.Combine(dir, $"{prefix}_selected.bmp"));

                        // Exercise each cell editor kind: glass TextBox, ComboBox
                        // (with its dropdown), and AutoSuggestBox (with suggestions).
                        await CaptureCellEditAsync(dataGrid, 0, Path.Combine(dir, $"{prefix}_edit_text.bmp"));
                        await CaptureCellEditAsync(dataGrid, 1, Path.Combine(dir, $"{prefix}_edit_combo.bmp"));
                        await CaptureCellEditAsync(dataGrid, 3, Path.Combine(dir, $"{prefix}_edit_suggest.bmp"));
                        dataGrid.SelectedIndex = -1;
                    }

                    if (tag == "commanding")
                    {
                        await CaptureMenuBarAsync(Path.Combine(dir, $"{prefix}_menubar.bmp"));
                        await CaptureCommandBarOverflowAsync(Path.Combine(dir, $"{prefix}_overflow.bmp"));
                    }

                    if (tag == "devstatus")
                    {
                        await CaptureGrowlAsync(Path.Combine(dir, $"{prefix}_growl.bmp"));
                    }

                    if (tag == "devdialogs")
                    {
                        await CaptureMessageBoxAsync(Path.Combine(dir, $"{prefix}_messagebox.bmp"));
                        await CaptureGlassWindowAsync(Path.Combine(dir, $"{prefix}_window.bmp"));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Screenshot automation failed: {ex}");
        }
        finally
        {
            Environment.Exit(0);
        }
    }

    private async Task CapturePickerFlyoutAsync<T>(string path) where T : FrameworkElement
    {
        var picker = FindDescendant<T>(ContentFrame);
        var flyoutButton = picker is null ? null : FindDescendant<Button>(picker, "FlyoutButton");
        if (flyoutButton is null)
        {
            Console.Error.WriteLine($"No FlyoutButton found for {typeof(T).Name}");
            return;
        }

        new Microsoft.UI.Xaml.Automation.Peers.ButtonAutomationPeer(flyoutButton).Invoke();
        await Task.Delay(900);

        var popup = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot)
            .FirstOrDefault(p => p.Child is not null);
        if (popup?.Child is FrameworkElement presenter)
        {
            await CaptureAsync(presenter, path);
            popup.IsOpen = false;
        }
        else
        {
            Console.Error.WriteLine($"No open popup found for {typeof(T).Name}");
        }

        await Task.Delay(300);
    }

    private async Task CaptureMenuFlyoutAsync(string path)
    {
        var menuButton = FindDescendant<Button>(ContentFrame, "MenuButton");
        if (menuButton?.Flyout is null)
        {
            Console.Error.WriteLine("No MenuButton with flyout found");
            return;
        }

        menuButton.Flyout.ShowAt(menuButton);
        await Task.Delay(900);

        var popup = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot)
            .FirstOrDefault(p => p.Child is not null);
        if (popup?.Child is FrameworkElement presenter)
        {
            await CaptureAsync(presenter, path);
        }

        menuButton.Flyout.Hide();
        await Task.Delay(300);
    }

    private async Task CaptureDialogAsync(string path)
    {
        var dialog = new ContentDialog
        {
            Title = "Liquid Glass dialog",
            Content = "Dialogs float on the heavier glass surface material.",
            PrimaryButtonText = "OK",
            CloseButtonText = "Cancel",
            XamlRoot = XamlRoot,
        };

        _ = dialog.ShowAsync();
        await Task.Delay(900);

        var popup = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot)
            .FirstOrDefault(p => p.Child is not null);
        if (popup?.Child is FrameworkElement dialogRoot)
        {
            await CaptureAsync(dialogRoot, path);
        }

        dialog.Hide();
        await Task.Delay(300);
    }

    private async Task CaptureGrowlAsync(string path)
    {
        // Same code path as the page's buttons: the page registered its host panel on Loaded.
        DevWinUI.LiquidGlass.Growl.Success("The deployment finished without errors.", "Deployed");
        DevWinUI.LiquidGlass.Growl.Warning("Disk space is running low on this volume.", "Low disk space");
        await Task.Delay(700);
        await CaptureAsync(this, path);
        await Task.Delay(4200); // let the toasts auto-dismiss so they don't bleed into later captures
    }

    private async Task CaptureMessageBoxAsync(string path)
    {
        if (XamlRoot is null)
        {
            return;
        }

        _ = DevWinUI.LiquidGlass.MessageBox.ShowAsync(
            XamlRoot,
            "Do you want to save your changes before closing?",
            "Save changes?",
            DevWinUI.LiquidGlass.MessageBoxButtons.YesNoCancel,
            DevWinUI.LiquidGlass.MessageBoxIcon.Question);
        await Task.Delay(900);

        var popup = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot)
            .FirstOrDefault(p => p.Child is not null);
        if (popup?.Child is FrameworkElement dialogRoot)
        {
            await CaptureAsync(dialogRoot, path);
            popup.IsOpen = false;
        }
        else
        {
            Console.Error.WriteLine("MessageBox popup did not open");
        }

        await Task.Delay(300);
    }

    private async Task CaptureMenuBarAsync(string path)
    {
        var item = FindDescendant<MenuBarItem>(ContentFrame);
        if (item is null)
        {
            Console.Error.WriteLine("No MenuBarItem found");
            return;
        }

        try
        {
            new Microsoft.UI.Xaml.Automation.Peers.MenuBarItemAutomationPeer(item).Invoke();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"MenuBarItem invoke failed: {ex.Message}");
            return;
        }

        await Task.Delay(700);
        var popup = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot)
            .FirstOrDefault(p => p.Child is not null);
        if (popup?.Child is FrameworkElement flyout)
        {
            await CaptureAsync(flyout, path);
            popup.IsOpen = false;
        }
        else
        {
            Console.Error.WriteLine("MenuBar flyout did not open");
        }

        await Task.Delay(300);
    }

    private async Task CaptureCommandBarOverflowAsync(string path)
    {
        var bar = FindDescendant<CommandBar>(ContentFrame);
        if (bar is null)
        {
            Console.Error.WriteLine("No CommandBar found");
            return;
        }

        bar.IsOpen = true;
        await Task.Delay(700);
        var popup = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot)
            .FirstOrDefault(p => p.Child is not null);
        if (popup?.Child is FrameworkElement overflow)
        {
            await CaptureAsync(overflow, path);
        }
        else
        {
            Console.Error.WriteLine("CommandBar overflow did not open");
        }

        bar.IsOpen = false;
        await Task.Delay(300);
    }

    private async Task CaptureCellEditAsync(CommunityToolkit.WinUI.UI.Controls.DataGrid dataGrid, int columnIndex, string path)
    {
        dataGrid.SelectedIndex = 1;
        dataGrid.CurrentColumn = dataGrid.Columns[columnIndex];
        await Task.Delay(200);
        if (!dataGrid.BeginEdit())
        {
            Console.Error.WriteLine($"BeginEdit failed for column {columnIndex}");
            return;
        }

        await Task.Delay(600);

        // Pop the editor's flyout so its glass surface is captured too. Popups
        // aren't part of the window visual tree, so RTB them separately.
        var popupExpected = false;
        if (FindDescendant<ComboBox>(dataGrid) is { } combo)
        {
            combo.IsDropDownOpen = true;
            popupExpected = true;
            await Task.Delay(500);
        }
        else if (FindDescendant<AutoSuggestBox>(dataGrid) is { } suggest)
        {
            suggest.ItemsSource = new[] { "Backdrop blur", "Brightness boost", "Saturation boost" };
            suggest.IsSuggestionListOpen = true;
            popupExpected = true;
            await Task.Delay(500);
        }

        await CaptureAsync(this, path);

        if (popupExpected)
        {
            var popup = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetOpenPopupsForXamlRoot(XamlRoot)
                .FirstOrDefault(p => p.Child is not null);
            if (popup?.Child is FrameworkElement flyout)
            {
                await CaptureAsync(flyout, Path.ChangeExtension(path, null) + "_flyout.bmp");
            }
            else
            {
                Console.Error.WriteLine($"No editor popup found for column {columnIndex}");
            }
        }

        if (FindDescendant<ComboBox>(dataGrid) is { } openCombo)
        {
            openCombo.IsDropDownOpen = false;
        }

        dataGrid.CancelEdit();
        await Task.Delay(300);
    }

    private async Task CaptureGlassWindowAsync(string path)
    {
        var window = DevWinUI.LiquidGlass.GlassWindowHelper.TryOpenWindow(
            "Liquid Glass window",
            (ContentFrame.Content as Pages.DevDialogsPage)?.BuildSecondaryWindowContent()
                ?? new Grid { Background = new SolidColorBrush(Microsoft.UI.Colors.DarkSlateBlue) });
        if (window is null)
        {
            Console.Error.WriteLine("Secondary window not supported on this platform");
            return;
        }

        await Task.Delay(900);
        if (window.Content is FrameworkElement root)
        {
            await CaptureAsync(root, path);
        }

        window.Close();
        await Task.Delay(300);
    }

    private static T? FindDescendant<T>(DependencyObject root, string? name = null) where T : FrameworkElement
    {
        var count = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < count; i++)
        {
            var child = Microsoft.UI.Xaml.Media.VisualTreeHelper.GetChild(root, i);
            if (child is T match && (name is null || match.Name == name))
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

    private static async Task CaptureAsync(UIElement element, string path)
    {
        var rtb = new Microsoft.UI.Xaml.Media.Imaging.RenderTargetBitmap();
        await rtb.RenderAsync(element);
        var buffer = await rtb.GetPixelsAsync();
        var pixels = System.Runtime.InteropServices.WindowsRuntime.WindowsRuntimeBufferExtensions.ToArray(buffer);
        WriteBmp(path, pixels, rtb.PixelWidth, rtb.PixelHeight);
        Console.WriteLine($"Captured {path} ({rtb.PixelWidth}x{rtb.PixelHeight})");
    }

    // Minimal 32bpp top-down BGRA bitmap writer (converted to PNG by tooling afterwards).
    // Pixels are premultiplied; composite over mid-gray so transparent captures stay readable.
    private static void WriteBmp(string path, byte[] bgra, int width, int height)
    {
        for (var i = 0; i < bgra.Length; i += 4)
        {
            var inverse = 255 - bgra[i + 3];
            if (inverse > 0)
            {
                bgra[i] = (byte)Math.Min(255, bgra[i] + 0x80 * inverse / 255);
                bgra[i + 1] = (byte)Math.Min(255, bgra[i + 1] + 0x80 * inverse / 255);
                bgra[i + 2] = (byte)Math.Min(255, bgra[i + 2] + 0x80 * inverse / 255);
            }
        }
        using var fs = File.Create(path);
        using var w = new BinaryWriter(fs);
        var imageSize = width * height * 4;
        w.Write((byte)'B'); w.Write((byte)'M');
        w.Write(14 + 40 + imageSize); // file size
        w.Write(0);                   // reserved
        w.Write(14 + 40);             // pixel data offset
        w.Write(40);                  // BITMAPINFOHEADER size
        w.Write(width);
        w.Write(-height);             // negative = top-down
        w.Write((short)1);            // planes
        w.Write((short)32);           // bpp
        w.Write(0);                   // no compression
        w.Write(imageSize);
        w.Write(2835); w.Write(2835); // 72 dpi
        w.Write(0); w.Write(0);       // palette
        w.Write(bgra);
    }
}

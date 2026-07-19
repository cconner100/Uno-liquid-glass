using DevWinUI.LiquidGlass;

namespace LiquidGlassGallery.Pages;

public sealed partial class DevDialogsPage : Page
{
    public DevDialogsPage()
    {
        this.InitializeComponent();
    }

    private async void OnShowInformation(object sender, RoutedEventArgs e)
        => await ShowMessageBoxAsync(
            "Liquid Glass dialogs pick up the heavier surface material from the app theme.",
            "Information",
            MessageBoxButtons.Ok,
            MessageBoxIcon.Information);

    private async void OnShowSuccess(object sender, RoutedEventArgs e)
        => await ShowMessageBoxAsync(
            "The operation completed successfully.",
            "Success",
            MessageBoxButtons.Ok,
            MessageBoxIcon.Success);

    private async void OnShowWarning(object sender, RoutedEventArgs e)
        => await ShowMessageBoxAsync(
            "Continuing will overwrite the existing file.",
            "Warning",
            MessageBoxButtons.OkCancel,
            MessageBoxIcon.Warning);

    private async void OnShowError(object sender, RoutedEventArgs e)
        => await ShowMessageBoxAsync(
            "The document could not be saved.",
            "Error",
            MessageBoxButtons.Ok,
            MessageBoxIcon.Error);

    private async void OnShowQuestion(object sender, RoutedEventArgs e)
        => await ShowMessageBoxAsync(
            "Do you want to save your changes before closing?",
            "Save changes?",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

    private async void OnShowFrameworkMessageDialog(object sender, RoutedEventArgs e)
    {
        if (_dialogOpen)
        {
            return;
        }

        _dialogOpen = true;
        try
        {
            var dialog = new Windows.UI.Popups.MessageDialog(
                "The framework adapter now resolves the Liquid Glass ContentDialog style.",
                "Framework MessageDialog");
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("OK"));
            dialog.Commands.Add(new Windows.UI.Popups.UICommand("Cancel"));
            InitializeMessageDialog(dialog);
            await dialog.ShowAsync();
        }
        finally
        {
            _dialogOpen = false;
        }
    }

    private static void InitializeMessageDialog(Windows.UI.Popups.MessageDialog dialog)
    {
        if (App.ActiveWindow is { } window)
        {
            WinRT.Interop.InitializeWithWindow.Initialize(
                dialog,
                WinRT.Interop.WindowNative.GetWindowHandle(window));
        }
    }

    // Only one ContentDialog may be open per XamlRoot; a double-click would
    // otherwise throw inside an async void handler and crash the app.
    private bool _dialogOpen;

    private async Task ShowMessageBoxAsync(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        if (XamlRoot is null || _dialogOpen)
        {
            return;
        }

        _dialogOpen = true;
        try
        {
            var result = await MessageBox.ShowAsync(XamlRoot, message, title, buttons, icon);
            MessageBoxResultText.Text = $"Result: {result}";
        }
        finally
        {
            _dialogOpen = false;
        }
    }

    private void OnOpenGlassWindow(object sender, RoutedEventArgs e)
    {
        var window = GlassWindowHelper.TryOpenWindow("Liquid Glass window", BuildGlassWindowContent());
        WindowFallbackText.Visibility = window is null ? Visibility.Visible : Visibility.Collapsed;
    }

    /// <summary>Exposes the window content for the screenshot automation in MainPage.</summary>
    internal UIElement BuildSecondaryWindowContent() => BuildGlassWindowContent();

    /// <summary>
    /// Builds the second window's content: a vibrant content layer (the same
    /// gradient backdrop idea as MainPage's BackgroundLayer) with a glass card
    /// floating above it. Application-level theme resources resolve in any
    /// window, so the card can reuse the Liquid Glass surface material.
    /// </summary>
    private static UIElement BuildGlassWindowContent()
    {
        var gradient = new LinearGradientBrush
        {
            StartPoint = new Windows.Foundation.Point(0, 0),
            EndPoint = new Windows.Foundation.Point(1, 1),
        };
        gradient.GradientStops.Add(new GradientStop { Offset = 0, Color = Windows.UI.Color.FromArgb(0xFF, 0x1B, 0x1B, 0x2F) });
        gradient.GradientStops.Add(new GradientStop { Offset = 0.5, Color = Windows.UI.Color.FromArgb(0xFF, 0x46, 0x2C, 0x71) });
        gradient.GradientStops.Add(new GradientStop { Offset = 1, Color = Windows.UI.Color.FromArgb(0xFF, 0x8E, 0x44, 0xAD) });

        var root = new Grid
        {
            Background = gradient,
        };

        var card = new Border
        {
            Background = LookupBrush("LiquidGlassSurfaceBrush")
                ?? new SolidColorBrush(Windows.UI.Color.FromArgb(0xE6, 0x2C, 0x2C, 0x2E)),
            BorderBrush = LookupBrush("LiquidGlassStrokeBrush"),
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(22),
            Padding = new Thickness(32, 24, 32, 24),
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Child = new TextBlock
            {
                Text = "A second Liquid Glass window",
                FontSize = 17,
                FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Microsoft.UI.Colors.White),
            },
        };

        root.Children.Add(card);
        return root;
    }

    private static Brush? LookupBrush(string key)
        => Application.Current?.Resources.TryGetValue(key, out var value) == true && value is Brush brush
            ? brush
            : null;
}

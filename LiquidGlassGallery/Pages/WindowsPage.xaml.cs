namespace LiquidGlassGallery.Pages;

public sealed partial class WindowsPage : Page
{
    public WindowsPage()
    {
        this.InitializeComponent();
    }

    // Only one ContentDialog may be open per XamlRoot; guard double-clicks.
    private bool _dialogOpen;

    private async void OnShowDialog(object sender, RoutedEventArgs e)
    {
        if (_dialogOpen)
        {
            return;
        }

        _dialogOpen = true;
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Liquid Glass dialog",
                Content = "Dialogs float on the heavier glass surface material.",
                PrimaryButtonText = "OK",
                CloseButtonText = "Cancel",
                XamlRoot = this.XamlRoot,
            };
            await dialog.ShowAsync();
        }
        finally
        {
            _dialogOpen = false;
        }
    }
}

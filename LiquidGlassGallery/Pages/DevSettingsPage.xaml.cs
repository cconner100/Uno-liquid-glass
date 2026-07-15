namespace LiquidGlassGallery.Pages;

public sealed partial class DevSettingsPage : Page
{
    public DevSettingsPage()
    {
        this.InitializeComponent();
    }

    private void OnCheckForUpdates(object sender, RoutedEventArgs e)
    {
        UpdatesCard.Description = $"Last checked at {DateTime.Now:T}";
    }
}

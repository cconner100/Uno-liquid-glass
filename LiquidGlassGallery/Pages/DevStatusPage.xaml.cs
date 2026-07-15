using DevWinUI.LiquidGlass;

namespace LiquidGlassGallery.Pages;

public sealed partial class DevStatusPage : Page
{
    private static readonly string[] StepHeaders = ["Account", "Shipping", "Payment", "Review"];

    public DevStatusPage()
    {
        this.InitializeComponent();

        Steps.ItemsSource = StepHeaders;

        Loaded += (_, _) => Growl.Register(GrowlHost);
        Unloaded += (_, _) => Growl.Unregister(GrowlHost);
    }

    private void OnPreviousStep(object sender, RoutedEventArgs e)
    {
        if (Steps.StepIndex > 0)
        {
            Steps.StepIndex--;
        }
    }

    private void OnNextStep(object sender, RoutedEventArgs e)
    {
        if (Steps.StepIndex < StepHeaders.Length - 1)
        {
            Steps.StepIndex++;
        }
    }

    private void OnGrowlSuccess(object sender, RoutedEventArgs e) =>
        Growl.Success("Your changes were saved to the cloud.", "Saved");

    private void OnGrowlInfo(object sender, RoutedEventArgs e) =>
        Growl.Info("A new version of the gallery is available.");

    private void OnGrowlWarning(object sender, RoutedEventArgs e) =>
        Growl.Warning("Storage is almost full — 92% used.");

    private void OnGrowlError(object sender, RoutedEventArgs e) =>
        Growl.Error("The connection to the sync service was lost.", "Sync failed");
}

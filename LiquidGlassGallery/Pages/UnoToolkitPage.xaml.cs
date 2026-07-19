namespace LiquidGlassGallery.Pages;

public sealed partial class UnoToolkitPage : Page
{
    public UnoToolkitPage()
    {
        InitializeComponent();
    }

    private void OnOpenDrawer(object sender, RoutedEventArgs e) => DemoDrawer.IsOpen = true;

    private void OnCloseDrawer(object sender, RoutedEventArgs e) => DemoDrawer.IsOpen = false;

    private void OnCloseDrawerFlyout(object sender, RoutedEventArgs e) => BottomDrawerFlyout.Hide();
}

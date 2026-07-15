namespace LiquidGlassGallery.Pages;

public sealed partial class ButtonsPage : Page
{
    private int _repeatCount;

    public ButtonsPage()
    {
        this.InitializeComponent();
    }

    private void OnRepeat(object sender, RoutedEventArgs e)
    {
        _repeatCount++;
        Repeater.Content = $"Hold: {_repeatCount}";
    }
}

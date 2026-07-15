using DevWinUI.LiquidGlass;

namespace LiquidGlassGallery.Pages;

public sealed partial class DevInputPage : Page
{
    public DevInputPage()
    {
        this.InitializeComponent();
    }

    private void OnPinChanged(object? sender, PinBoxPasswordChangedEventArgs e)
    {
        PinValueText.Text = string.IsNullOrEmpty(e.NewPassword)
            ? "Entered: (empty)"
            : $"Entered: {e.NewPassword}";
    }
}

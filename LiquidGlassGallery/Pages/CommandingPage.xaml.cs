namespace LiquidGlassGallery.Pages;

public sealed partial class CommandingPage : Page
{
    private int _created;

    public CommandingPage()
    {
        this.InitializeComponent();
    }

    private void OnSplitButtonClick(Microsoft.UI.Xaml.Controls.SplitButton sender, Microsoft.UI.Xaml.Controls.SplitButtonClickEventArgs args)
    {
        _created++;
        SplitStatus.Text = $"Created \"{NewItemSplitButton.Content}\" ({_created} item{(_created == 1 ? "" : "s")} so far).";
    }

    private void OnNewNote(object sender, RoutedEventArgs e) => SetDefaultAction("New Note");

    private void OnNewFolder(object sender, RoutedEventArgs e) => SetDefaultAction("New Folder");

    private void OnNewTag(object sender, RoutedEventArgs e) => SetDefaultAction("New Tag");

    private void SetDefaultAction(string action)
    {
        NewItemSplitButton.Content = action;
        _created++;
        SplitStatus.Text = $"Created \"{action}\" — it is now the split button's default action.";
    }
}

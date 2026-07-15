namespace LiquidGlassGallery.Pages;

public sealed partial class TextInputPage : Page
{
    private static readonly string[] _fruits =
        ["Apple", "Apricot", "Banana", "Blueberry", "Cherry", "Grape", "Mango", "Orange", "Peach", "Pear", "Plum", "Raspberry", "Strawberry"];

    public TextInputPage()
    {
        this.InitializeComponent();
    }

    private void OnSuggestTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            sender.ItemsSource = _fruits
                .Where(f => f.Contains(sender.Text, StringComparison.OrdinalIgnoreCase))
                .ToArray();
        }
    }
}

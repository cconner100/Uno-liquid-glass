using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.WinUI.Controls;
using Microsoft.UI.Xaml.Controls;

namespace LiquidGlassGallery.Pages;

public sealed partial class ToolkitInputPage : Page
{
    private readonly ObservableCollection<string> _tags = new() { "Glass", "Acrylic", "Uno" };

    private static readonly string[] AllTags =
    [
        "Acrylic", "Blur", "Capsule", "Concentric", "Frosted", "Glass",
        "Liquid", "Specular", "Tint", "Translucent", "Uno", "WinUI",
    ];

    public ToolkitInputPage()
    {
        this.InitializeComponent();

        TagBox.ItemsSource = _tags;
        TagBox.SelectedIndex = 1;
        DisabledTagBox.ItemsSource = new[] { "Disabled", "Token" };
    }

    private void OnTagTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput)
        {
            return;
        }

        var text = sender.Text ?? string.Empty;
        TagBox.SuggestedItemsSource = AllTags
            .Where(tag => tag.Contains(text, StringComparison.OrdinalIgnoreCase)
                          && !_tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
            .OrderBy(tag => tag)
            .ToList();
    }

    private void OnRangeChanged(object? sender, RangeChangedEventArgs e)
    {
        if (RangeValueText is not null)
        {
            RangeValueText.Text = $"Selected range: {PriceRange.RangeStart:0} – {PriceRange.RangeEnd:0}";
        }
    }

    // The toolkit's picker (CommunityToolkit.WinUI.Controls.ColorPicker) derives from the
    // WinUI one, so the event args come from Microsoft.UI.Xaml.Controls; the sender type is
    // fully qualified to dodge the ColorPicker name collision between the two namespaces.
    private void OnColorChanged(Microsoft.UI.Xaml.Controls.ColorPicker sender, ColorChangedEventArgs args)
    {
        if (ColorHexText is not null)
        {
            var c = args.NewColor;
            ColorHexText.Text = $"Selected: #{c.R:X2}{c.G:X2}{c.B:X2}";
        }
    }
}

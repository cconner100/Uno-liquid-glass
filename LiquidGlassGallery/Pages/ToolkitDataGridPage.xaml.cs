using System.Collections.ObjectModel;
using CommunityToolkit.WinUI.UI.Controls;

namespace LiquidGlassGallery.Pages;

public sealed partial class ToolkitDataGridPage : Page
{
    public sealed class GlassMaterial
    {
        public string Material { get; set; } = "";
        public string Layer { get; set; } = "";
        public int BlurRadius { get; set; }
        public double Opacity { get; set; }
        public string Effect { get; set; } = "";
    }

    private static readonly string[] _layers =
    [
        "Controls", "Media overlays", "Text inputs", "Flyouts & dialogs", "Sidebars", "Primary actions", "Picker wheels", "Edges",
    ];

    private static readonly string[] _effects =
    [
        "Backdrop blur", "Brightness boost", "Saturation boost", "Specular highlight", "Shadow", "Rim light", "None",
    ];

    private readonly ObservableCollection<GlassMaterial> _materials =
    [
        new() { Material = "Regular glass", Layer = "Controls", BlurRadius = 24, Opacity = 0.45, Effect = "Backdrop blur" },
        new() { Material = "Clear glass", Layer = "Media overlays", BlurRadius = 16, Opacity = 0.15, Effect = "Saturation boost" },
        new() { Material = "Control fill", Layer = "Text inputs", BlurRadius = 12, Opacity = 0.30, Effect = "Backdrop blur" },
        new() { Material = "Surface", Layer = "Flyouts & dialogs", BlurRadius = 32, Opacity = 0.70, Effect = "Shadow" },
        new() { Material = "Pane", Layer = "Sidebars", BlurRadius = 28, Opacity = 0.35, Effect = "Backdrop blur" },
        new() { Material = "Prominent", Layer = "Primary actions", BlurRadius = 8, Opacity = 0.95, Effect = "Brightness boost" },
        new() { Material = "Highlight", Layer = "Picker wheels", BlurRadius = 0, Opacity = 0.16, Effect = "None" },
        new() { Material = "Specular rim", Layer = "Edges", BlurRadius = 0, Opacity = 0.70, Effect = "Rim light" },
    ];

    public ToolkitDataGridPage()
    {
        this.InitializeComponent();
        Grid.ItemsSource = _materials;
        LayerColumn.ItemsSource = _layers;
    }

    // The DataGrid raises Sorting but leaves the actual ordering to the app.
    private void OnSorting(object sender, DataGridColumnEventArgs e)
    {
        var ascending = e.Column.SortDirection != DataGridSortDirection.Ascending;
        e.Column.SortDirection = ascending ? DataGridSortDirection.Ascending : DataGridSortDirection.Descending;

        foreach (var column in Grid.Columns)
        {
            if (column != e.Column)
            {
                column.SortDirection = null;
            }
        }

        Comparison<GlassMaterial> comparison = (e.Column.Tag as string) switch
        {
            "Layer" => (a, b) => string.Compare(a.Layer, b.Layer, StringComparison.OrdinalIgnoreCase),
            "BlurRadius" => (a, b) => a.BlurRadius.CompareTo(b.BlurRadius),
            "Effect" => (a, b) => string.Compare(a.Effect, b.Effect, StringComparison.OrdinalIgnoreCase),
            _ => (a, b) => string.Compare(a.Material, b.Material, StringComparison.OrdinalIgnoreCase),
        };

        var sorted = _materials.ToList();
        sorted.Sort(ascending ? comparison : (a, b) => comparison(b, a));
        for (var i = 0; i < sorted.Count; i++)
        {
            var currentIndex = _materials.IndexOf(sorted[i]);
            if (currentIndex != i)
            {
                _materials.Move(currentIndex, i);
            }
        }
    }

    private void OnRowSelected(object sender, SelectionChangedEventArgs e)
    {
        SelectionText.Text = Grid.SelectedItem is GlassMaterial m
            ? $"Selected: {m.Material} — {m.Layer}, blur {m.BlurRadius}, {m.Effect}"
            : "No row selected.";
    }

    private void OnCellEditEnded(object sender, DataGridCellEditEndedEventArgs e)
    {
        // GlassMaterial is a plain POCO (no INotifyPropertyChanged), so refresh
        // the selection summary manually after an edit commits.
        OnRowSelected(sender, null!);
    }

    private void OnAddRow(object sender, RoutedEventArgs e)
    {
        var item = new GlassMaterial { Material = "New material", Layer = "Controls", BlurRadius = 20, Opacity = 0.5, Effect = "None" };
        _materials.Add(item);
        Grid.SelectedItem = item;
        Grid.ScrollIntoView(item, null);
    }

    private void OnRemoveRow(object sender, RoutedEventArgs e)
    {
        if (Grid.SelectedItem is GlassMaterial m)
        {
            _materials.Remove(m);
        }
    }

    private void OnEffectSuggestions(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
        {
            sender.ItemsSource = _effects
                .Where(x => x.Contains(sender.Text, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }
    }

    private void OnEffectChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        => sender.Text = args.SelectedItem?.ToString() ?? sender.Text;
}

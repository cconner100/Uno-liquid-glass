namespace LiquidGlassGallery.Pages;

public sealed partial class SelectionPage : Page
{
    public SelectionPage()
    {
        this.InitializeComponent();
        DashboardSelector.SelectedItem = OverviewSelectorItem;
    }

    private void OnDashboardSelectionChanged(SelectorBar sender, SelectorBarSelectionChangedEventArgs args)
    {
        if (sender.SelectedItem is not SelectorBarItem selectedItem)
        {
            return;
        }

        DashboardContentTitle.Text = selectedItem.Text;
        DashboardContentDescription.Text = selectedItem.Tag as string ?? string.Empty;
    }
}

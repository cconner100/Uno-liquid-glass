namespace LiquidGlassGallery.Pages;

public sealed partial class ContainersPage : Page
{
    public ContainersPage()
    {
        this.InitializeComponent();

        // BreadcrumbBar path (BreadcrumbBarItem containers are generated per string).
        Crumbs.ItemsSource = new[] { "Home", "Library", "Liquid Glass", "Containers" };

        // Small 2-level tree built via TreeViewNode (the Uno-reliable route).
        var documents = new TreeViewNode { Content = "Documents", IsExpanded = true };
        documents.Children.Add(new TreeViewNode { Content = "Design specs" });
        documents.Children.Add(new TreeViewNode { Content = "Meeting notes" });

        var media = new TreeViewNode { Content = "Media", IsExpanded = true };
        media.Children.Add(new TreeViewNode { Content = "Photos" });
        media.Children.Add(new TreeViewNode { Content = "Wallpapers" });

        var downloads = new TreeViewNode { Content = "Downloads" };
        downloads.Children.Add(new TreeViewNode { Content = "Installers" });

        SampleTree.RootNodes.Add(documents);
        SampleTree.RootNodes.Add(media);
        SampleTree.RootNodes.Add(downloads);

        // Keep the pips pager and the FlipView in sync (guards avoid feedback loops).
        Slides.SelectionChanged += (_, _) =>
        {
            if (Slides.SelectedIndex >= 0 && SlidePips.SelectedPageIndex != Slides.SelectedIndex)
            {
                SlidePips.SelectedPageIndex = Slides.SelectedIndex;
            }
        };
        SlidePips.SelectedIndexChanged += (_, _) =>
        {
            if (SlidePips.SelectedPageIndex >= 0 && Slides.SelectedIndex != SlidePips.SelectedPageIndex)
            {
                Slides.SelectedIndex = SlidePips.SelectedPageIndex;
            }
        };
    }

    private void OnPaneToggleClick(object sender, RoutedEventArgs e)
        => DemoSplitView.IsPaneOpen = PaneToggle.IsChecked == true;
}

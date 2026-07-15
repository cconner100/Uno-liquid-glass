using System.Windows.Input;
using CommunityToolkit.WinUI.Controls;

namespace LiquidGlassGallery.Pages;

public sealed partial class ToolkitSettingsPage : Page
{
    public ToolkitSettingsPage()
    {
        this.InitializeComponent();

        Metadata.Items = new List<MetadataItem>
        {
            new MetadataItem { Label = "Windows Community Toolkit" },
            new MetadataItem
            {
                Label = "v8.3",
                Command = new ActionCommand(_ =>
                    MetadataOutput.Text = $"Version link invoked at {DateTime.Now:T}."),
            },
            new MetadataItem { Label = "MIT license" },
        };
    }

    private void OnCheckForUpdates(object sender, RoutedEventArgs e)
    {
        UpdatesCard.Description = $"Last checked at {DateTime.Now:T}";
    }

    /// <summary>Minimal ICommand for the MetadataItem sample (no MVVM package dependency).</summary>
    private sealed class ActionCommand : ICommand
    {
        private readonly Action<object?> _execute;

        public ActionCommand(Action<object?> execute) => _execute = execute;

#pragma warning disable CS0067 // Never raised — the command is always executable.
        public event EventHandler? CanExecuteChanged;
#pragma warning restore CS0067

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter) => _execute(parameter);
    }
}

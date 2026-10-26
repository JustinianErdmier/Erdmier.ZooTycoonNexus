namespace Erdmier.ZooTycoonNexus.Desktop.Views.Dialogs;

/// <summary>The modal About dialogue.</summary>
public sealed partial class AboutDialogView : ClassicWindow
{
    /// <summary>Initialises a new instance.</summary>
    public AboutDialogView() => AvaloniaXamlLoader.Load(this);

    private void OnOkClick(object? sender, RoutedEventArgs e) => Close();
}

namespace Erdmier.ZooTycoonNexus.Desktop.Tests.Unit.Boot;

/// <summary>Verifies the main window's Help → About command and its build-derived version text.</summary>
public sealed class MainWindowViewModelAboutTests
{
    [ Fact ]
    public async Task OpenAboutCommand_ShowsTheAboutDialogueOnce()
    {
        IDialogService dialogs = Substitute.For<IDialogService>();

        MainWindowViewModel window = Create(dialogs, Substitute.For<IProductInfo>());

        await window.OpenAboutCommand.ExecuteAsync(parameter: null);

        await dialogs.Received(requiredNumberOfCalls: 1).ShowAboutAsync();
    }

    [ Fact ]
    public void VersionText_IsVPlusTheProductVersion()
    {
        IProductInfo productInfo = Substitute.For<IProductInfo>();

        productInfo.Version.Returns("2.4.6");

        MainWindowViewModel window = Create(Substitute.For<IDialogService>(), productInfo);

        window.VersionText.ShouldBe(expected: "V2.4.6");
    }

    private static MainWindowViewModel Create(IDialogService dialogs, IProductInfo productInfo)
        => new(Substitute.For<IMediator>(),
               Substitute.For<IApplicationLifecycle>(),
               dialogs,
               new WeakReferenceMessenger(),
               NullLogger<MainWindowViewModel>.Instance,
               NullLogger<InstallationGridViewModel>.Instance,
               NullLogger<PlayViewModel>.Instance,
               NullLogger<IniConfigTabViewModel>.Instance,
               NullLogger<IniEditorViewModel>.Instance,
               NullLogger<IniPlaceholderViewModel>.Instance,
               new IniEditorStyleState(),
               productInfo);
}

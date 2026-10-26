# About Dialogue Implementation Plan

**Date:** 2026-10-25

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task.
> Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make Help → About open the §9.9 About dialogue, and make the version a build setting read back at runtime, so the title bar drops it and the status bar binds to
it.

**Architecture:** `Directory.Build.props` sets `Version`, `Authors` and `Copyright`; the Desktop project stamps a `BuildDateUtc` assembly metadata item. A Desktop
`IProductInfo` singleton (`AssemblyProductInfo`) reads those attributes once. `AboutDialogViewModel` and `MainWindowViewModel.VersionText` format them, and
`IDialogService.ShowAboutAsync` opens the new `AboutDialogView`.

**Tech Stack:** .NET 10 / MSBuild, Avalonia 11.3 + Classic.Avalonia, CommunityToolkit.Mvvm, Microsoft.Extensions.DependencyInjection, xUnit 2.9 + Shouldly + NSubstitute,
Avalonia.Headless.XUnit.

**Spec:** [`Docs/Specs/Epic-1-MVP/milestone-15-about-dialogue-design.md`](../../Specs/Epic-1-MVP/milestone-15-about-dialogue-design.md). Read it before any task. Decisions
1–5 and calls C1–C10 are binding, and the §4.5 copy is used verbatim.

## Global Constraints

- Branch `feat/milestone-15-about-dialogue`. **Commit every task automatically**; do not wait for review. Unsigned (`git -c commit.gpgsign=false commit`), message ending
  with `Co-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`; stage only the task's files.
- Commit format `type(emoji): subject` (`feat`→✨, `fix`→🐛, `refactor`→♻️, `test`→✅, `docs`→📝, `build`→📦), British English.
- **PowerShell** for `dotnet` and `git`; commit messages through a Git Bash heredoc. Foreground commands only. On a file-lock error, stop and report.
- No filesystem-wide searches. **Never write, rename or delete anything under `SampleInstallations/`.**
- `TreatWarningsAsErrors` and `Nullable` on. One type per file; folder mirrors namespace; file-scoped namespaces; XML docs on public members; spaced attributes; named
  arguments on literals in tests; every XAML file keeps `x:DataType`; `MainWindow.axaml` ≤ 100 lines (it is exactly 100 today, so edits must replace, not add);
  GlobalUsings over local usings. Do not add `[ UsedImplicitly ]`.
- Views use tokens through `{StaticResource …}` / `{tokens:Space …}`; no new literal colours, fonts, sizes or spacing (the architecture tests enforce it). The two numbers
  the spec fixes, window `Width="430"` and the 64 px logo, follow the existing precedent (`ErrorMessageView` `Width="420"`, the `IconLarge` class); add a size class or token
  for the 64 px logo if the architecture tests reject a literal.
- **The release number appears in exactly one file, `Directory.Build.props`.** No view, view model or test may carry `1.0.0` as the app's version (test fakes use other
  numbers, e.g. `2.4.6`).
- Product copy, verbatim: window title `  About Zoo Tycoon Nexus`; `Zoo Tycoon Nexus`; `Version {Version} (build {yyyy.MM.dd})`;
  `A native Windows launcher and configuration tool for Zoo Tycoon (2001).`; `Built with .NET 10, Avalonia 11.3, Classic.Avalonia, SQLite via EF Core and Serilog.`;
  `© 2026 Justinian Erdmier`; `OK`. Main window title `  Zoo Tycoon Nexus`; status cell `V{Version}`.
- Markdown wrapped at 180 characters (PowerShell `.Length`); the ledger `Docs/Ledgers/Epic-1-MVP/milestone-15-ledger.md` is the controller's.

## Review Focus

1. **An informational version carrying `+<commit sha>`**: the .NET SDK adds it whenever source-revision data is present, so a CI or Release build can show
   `1.0.0+65e5c3d…`. The UI must show `1.0.0`. Pinned in Task 1 (`NormaliseVersion_*`) and the real-assembly test (no `+`).
2. **An assembly missing the stamped attributes** (a stripped or hand-built binary): the launcher must still start. It shows `0.0.0`, a min date and an empty copyright,
   and logs a warning, never throws. Pinned in Task 1 (`MissingAttributes_FallBackAndWarn`, against a dynamic assembly with no attributes).
3. **A player whose Windows culture uses a non-Gregorian calendar or different digits** (e.g. `th-TH`, `ar-SA`): the build stamp must still read `2027.01.09`. Pinned
   in Task 2 (`VersionLine_IgnoresCurrentCulture`).
4. **Closing with the keyboard:** Enter and Esc must both close the box, because OK is the only button. Pinned in Task 2 (`OkButton_IsDefaultAndCancel`, headless).
5. **A long copyright or attribution line** must wrap, not widen the 430 px window. Pinned in Task 2 (`LongCopy_WrapsWithinWindowWidth`, headless).

---

## Task 0: Branch, ledger and before screenshots (controller)

- [x] **Step 1:** Create `feat/milestone-15-about-dialogue` from `main`; create the ledger and register it in the `.slnx` (`/Docs/Ledgers/Epic-1-MVP/`, after
  milestone 14). Capture the main window (title bar and status bar) from the current build.

---

## Task 1: Build-stamped product information (`IProductInfo`)

**Files:**
- Modify: `Directory.Build.props` (existing `PropertyGroup`): `<Version>1.0.0</Version>`, `<Authors>Justinian Erdmier</Authors>`,
  `<Copyright>© 2026 Justinian Erdmier</Copyright>`.
- Modify: `Source/Erdmier.ZooTycoonNexus.Desktop/Erdmier.ZooTycoonNexus.Desktop.csproj`: an `ItemGroup` with
  `<AssemblyMetadata Include="BuildDateUtc" Value="$([System.DateTime]::UtcNow.ToString('yyyy-MM-dd'))" />` (any expression yielding UTC `yyyy-MM-dd` is acceptable).
- Create: `Source/Erdmier.ZooTycoonNexus.Desktop/Composition/IProductInfo.cs`, `Composition/AssemblyProductInfo.cs`.
- Modify: `Composition/DesktopServiceCollectionExtensions.cs`: register the singleton.
- Test: `Tests/Erdmier.ZooTycoonNexus.Desktop.Tests.Unit/Composition/AssemblyProductInfoTests.cs`.

**Interfaces — Produces:**
- `public interface IProductInfo { string Version { get; } DateOnly BuildDateUtc { get; } string Copyright { get; } }`.
- `public sealed class AssemblyProductInfo : IProductInfo`, with:
  - `AssemblyProductInfo(Assembly assembly, ILogger<AssemblyProductInfo> logger)`, which reads all three values once, in the constructor.
  - `public static string NormaliseVersion(string informationalVersion)`, which drops everything from the first `+`.
  - Registration: `services.AddSingleton<IProductInfo>(sp => new AssemblyProductInfo(typeof(App).Assembly, sp.GetRequiredService<ILogger<AssemblyProductInfo>>()))`.
  - Fallbacks: version `0.0.0`, date `DateOnly.MinValue`, copyright `""`. Each logs one warning naming the missing attribute. The date is parsed with
    `DateOnly.ParseExact(value, "yyyy-MM-dd", CultureInfo.InvariantCulture)` through `TryParseExact`.

- [ ] **Step 1: Write the failing tests**
  - `NormaliseVersion_StripsBuildMetadata`, as an `[ Theory ]`: `1.2.3+abc123` → `1.2.3`; `1.2.3-beta.1+abc` → `1.2.3-beta.1`; `1.2.3` → `1.2.3`.
  - `DesktopAssembly_VersionMatchesDirectoryBuildProps`. Locate `Directory.Build.props` by walking up from `AppContext.BaseDirectory`, read `<Version>` with
    `XDocument`, then `new AssemblyProductInfo(typeof(App).Assembly, NullLogger<AssemblyProductInfo>.Instance).Version.ShouldBe(propsVersion)`. Add
    `.ShouldNotContain("+")`.
  - `DesktopAssembly_BuildDateIsRecent`: the date is not `DateOnly.MinValue`, and lies between `DateOnly.FromDateTime(DateTime.UtcNow).AddDays(-1)` and today (UTC),
    inclusive (C10).
  - `DesktopAssembly_CopyrightIsStamped`: `Copyright.ShouldStartWith("©")`.
  - `MissingAttributes_FallBackAndWarn`. Use an `AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("NoProductInfo"), AssemblyBuilderAccess.Run)` with a
    `RecordingLogger<AssemblyProductInfo>` (existing in `Tests/…/Common/`). It reports `0.0.0`, `DateOnly.MinValue` and `""`, logs warnings, and does not throw.
- [ ] **Step 2: Run** `dotnet test Tests/Erdmier.ZooTycoonNexus.Desktop.Tests.Unit --filter FullyQualifiedName~AssemblyProductInfoTests`. Expected: the build fails,
  because `AssemblyProductInfo` is not defined.
- [ ] **Step 3: Implement** the props, csproj, interface, class and registration above. Add `global using System.Reflection;` to the Desktop and test `GlobalUsings.cs`
  if not already present.
- [ ] **Step 4: Run** the same filter, then the Desktop and Architecture test projects. Expected: all pass. Build the Desktop project and confirm in the generated
  `obj/…/Erdmier.ZooTycoonNexus.Desktop.AssemblyInfo.cs` that `BuildDateUtc` and `AssemblyCopyright` are present.
- [ ] **Step 5: Commit**: `feat(✨): stamp the version, build date and copyright into the Desktop assembly`

---

## Task 2: The About dialogue view pair

**Files:**
- Create: `Source/Erdmier.ZooTycoonNexus.Desktop/ViewModels/Dialogs/AboutDialogViewModel.cs`, `Views/Dialogs/AboutDialogView.axaml` + `.axaml.cs`.
- Modify: `Composition/DesktopServiceCollectionExtensions.cs`: `services.AddTransient<AboutDialogViewModel>();`.
- Test: `Tests/…Desktop.Tests.Unit/Dialogs/AboutDialogViewModelTests.cs`, `Tests/…/Views/AboutDialogViewTests.cs`, and `Views/ProductNameTests.cs` (new case).

**Interfaces:**
- Consumes: `IProductInfo` (Task 1).
- Produces: `public sealed class AboutDialogViewModel : ViewModelBase`, with:
  - `AboutDialogViewModel(IProductInfo productInfo)`, plus a parameterless designer constructor over a `file`-scoped `IProductInfo` reporting `1.0.0`,
    `new DateOnly(2026, 10, 25)` and `© 2026 Justinian Erdmier`. This is the one place a `1.0.0` may appear outside the props file: it is designer data, not the app's
    version.
  - Read-only `string` properties, computed once: `ProductName`, `VersionLine`, `Description`, `Attribution` and `Copyright`.
  - `VersionLine` is `$"Version {Version} (build {BuildDateUtc.ToString("yyyy.MM.dd", CultureInfo.InvariantCulture)})"`.

- [ ] **Step 1: Write the failing tests**
  - `VersionLine_FormatsVersionAndBuildDate`: a fake reporting `2.4.6` and `new DateOnly(2027, 1, 9)` gives `Version 2.4.6 (build 2027.01.09)`.
  - `VersionLine_IgnoresCurrentCulture`: the same, with `CultureInfo.CurrentCulture` set to `th-TH` and then `ar-SA` (restore it in `finally`).
  - `Copyright_PassesThrough`; `ProductName_Description_Attribution_AreTheSpecCopy`, with the three strings from the Global Constraints verbatim.
  - `AboutDialog_Title_IsAboutZooTycoonNexus` (in `ProductNameTests`): `"  About Zoo Tycoon Nexus"`.
  - Headless (`[ AvaloniaFact ]`, `AboutDialogViewTests`):
    - `OkButton_IsDefaultAndCancel`: find the button whose `Content` is `OK` and assert `IsDefault` and `IsCancel`.
    - `LongCopy_WrapsWithinWindowWidth`: a fake copyright of 300 characters. After `Show()` and `RunJobs()`, `view.Bounds.Width` is still `430`.
    - `Logo_IsPrimaryIconAt64`: the `Image` source path ends with `ZTL_Primary-Icon.png`, and its `Width` and `Height` are `64`.
- [ ] **Step 2: Run** the Desktop test project filtered to `AboutDialog`. Expected: the build fails.
- [ ] **Step 3: Implement** the view model and the view per spec §4.5, in `ErrorMessageView`'s mould:
  - **Window:** `ClassicWindow`, `CanResize="False"`, `ShowInTaskbar="False"`, `SizeToContent="Height"`, `WindowStartupLocation="CenterOwner"`, `Width="430"`,
    icon `ZTL_Title-Icon.png`, and the minimise/restore caption-collapse styles.
  - **Header:** a `DockPanel` with the 64 px logo docked left and top-aligned. Beside it, a `StackPanel` holding the name (`FontSizeAboutTitle`, `FontWeightStrong`),
    the version line, and the muted, wrapping description.
  - **Body:** the etched separator other dialogues use, then the attribution and the copyright as two wrapping lines.
  - **Footer:** a right-aligned footer (conventions §5) with one `OK` `DialogButton`, `IsDefault="True"` and `IsCancel="True"`, closing from a code-behind click
    handler.
  - Every `TextBlock` with copy wraps.
- [ ] **Step 4: Run** the Desktop and Architecture test projects. Expected: all pass, including `ViewLoadTests`, which picks up the new view, and the view-pair rule.
- [ ] **Step 5: Commit**: `feat(✨): add the About dialogue`

---

## Task 3: Wire Help → About and the main-window version

**Files:**
- Modify: `Composition/IDialogService.cs` and `Composition/AvaloniaDialogService.cs`.
- Modify: the three `file`-scoped `NoOpDialogService`s in `ViewModels/Boot/PlayViewModel.cs`, `ViewModels/Tabs/IniConfigTabViewModel.cs` and
  `ViewModels/IniConfig/IniEditorViewModel.cs`.
- Modify: `ViewModels/MainWindowViewModel.cs` and `Views/MainWindow.axaml` (lines 10, 64, 79 and 96).
- Test: `Tests/…/Boot/MainWindowViewModelAboutTests.cs` (new) and `Views/ProductNameTests.cs`.
- Test: the six constructions that gain the new argument:
  - `Boot/MainWindowViewModelGuardTests.cs`, `Boot/MainWindowViewModelOptionsTests.cs`, `Boot/MainWindowViewModelStatusTests.cs` and
    `Boot/MainWindowViewModelStyleMenuTests.cs`.
  - `Views/MainWindowStyleMenuTests.cs` and `Views/ViewLoadTests.cs` (`CreateMainWindowViewModel`).

**Interfaces:**
- Consumes: `IProductInfo` (Task 1); `AboutDialogViewModel` and `AboutDialogView` (Task 2).
- Produces:
  - `Task IDialogService.ShowAboutAsync()`. Its XML doc reads "Shows the modal About dialogue."
  - `MainWindowViewModel`:
    - takes `IProductInfo productInfo` as a new **last** constructor parameter;
    - exposes `public string VersionText { get; }`, which is `$"V{productInfo.Version}"`;
    - gains a `[ RelayCommand ] private Task OpenAboutAsync() => _dialogs.ShowAboutAsync();`, generating `OpenAboutCommand`, with a comment in the style of
      `OpenOptionsAsync`'s.

- [ ] **Step 1: Write the failing tests**
  - `OpenAboutCommand_ShowsTheAboutDialogueOnce`, modelled on `OpenOptionsCommand_ShowsTheOptionsDialogueOnce`.
  - `VersionText_IsVPlusTheProductVersion`: a substitute reporting `2.4.6` gives `V2.4.6`.
  - `MainWindow_Title_NamesZooTycoonNexus`: the expectation changes to `"  Zoo Tycoon Nexus"`.
  - The six existing constructions pass `Substitute.For<IProductInfo>()`.
- [ ] **Step 2: Run** the Desktop test project. Expected: the build fails.
- [ ] **Step 3: Implement**:
  - `AvaloniaDialogService.ShowAboutAsync` follows `ShowOptionsAsync` without `InitialiseAsync`: resolve the owner, returning if it is null. Then resolve
    `AboutDialogViewModel` from DI, and `await new AboutDialogView { DataContext = vm }.ShowDialog(owner)`.
  - The no-op services return `Task.CompletedTask`.
  - `MainWindow.axaml`:
    - line 10 becomes `Title="  Zoo Tycoon Nexus"`;
    - line 64 becomes `<MenuItem Header="Contents" /> <Separator /> <MenuItem Header="About" Command="{Binding OpenAboutCommand}" />`, still one line (R11);
    - lines 79 and 96 become `<TextBlock Text="{Binding VersionText}" />`;
    - the file stays at 100 lines.
- [ ] **Step 4: Run** every test project, then the full solution build. Expected: all pass, 0 warnings. Then search `Source/` (excluding `bin/` and `obj/`) for
  `(?<![\d.])1\.0\.0(?![\d.])` in `*.axaml` and `*.cs`, for example `Get-ChildItem Source -Recurse -Include *.axaml,*.cs | Where-Object FullName -notmatch '\\(bin|obj)\\' |
  Select-String '(?<![\d.])1\.0\.0(?![\d.])'`. It must list only the Task 2 designer constructor.
- [ ] **Step 5: Commit**: `feat(✨): open About from the Help menu and read the version from the build`

---

## Task 4: Documentation as built

**Files:**
- Modify: `Docs/Specs/1-zoo-tycoon-nexus-master-design.md`.
- Modify: `Docs/UserInterfaceDesign/conventions.md`.
- Modify: `Docs/TestPlans/manual-end-to-end-tests.md`.

- [ ] **Step 1: SDD** (spec §5):
  - **§9.1:** the title bar becomes `Zoo Tycoon Nexus` with no version. The status-bar bullet says the version label is `V` + the build's version, read through
    `IProductInfo`.
  - **§9.9:** rewritten as built. Name the view pair, the logo asset and size, the §4.5 copy, the version and build-date sources, and OK as default + cancel.
  - **§9.10:** the About row gets command `OpenAboutCommand` and the note "Opens the §9.9 dialogue".
  - **§11.4:** list the new test classes.
  - **§13.5 DW-23:** narrow it to the status messages and the repeated "Cannot Reload zoo.ini" title.
  - **Document control:** a 1.19 row for milestone 15 that names the sections changed.
- [ ] **Step 2: conventions §4 icon table:** the About row becomes `ZTL_Title-Icon.png` (title bar) / `ZTL_Primary-Icon.png` (body, 64 px), with the *(not built)* note
  removed.
- [ ] **Step 3: Manual test plan:** add an About case:
  - Help → About opens it, centred on the main window.
  - The version matches the status bar.
  - The build date is the build's own.
  - OK, Enter, Esc and ✕ each close it.
  - The title bar shows no version.
- [ ] **Step 4: Verify** that every changed line of prose is ≤ 180 characters (PowerShell `.Length`). **Commit**: `docs(📝): document the About dialogue and the
  build-stamped version`

---

## Task 5: Check it in the running app (controller)

- [ ] **Step 1:** Run the launcher against a sample installation. Open Help → About and compare it with the prototype's header and footer
  (`dialogs.jsx:356`, header and footer only). Confirm the version and build date, close the box with OK, Enter, Esc and ✕, and confirm the title bar and both status bars
  (booting and booted).
- [ ] **Step 2:** Close the launcher.

## Verification

- [ ] Clean build with 0 warnings; every test project passes.
- [ ] `Directory.Build.props` is the only place the release number is set; the Task 3 search finds nothing under `Source/` but the designer constructor.
- [ ] Task 5 checked in the running app, or recorded in the ledger as left to the maintainer.
- [ ] Final whole-branch review and fix waves per CLAUDE.md; then push and open the pull request.

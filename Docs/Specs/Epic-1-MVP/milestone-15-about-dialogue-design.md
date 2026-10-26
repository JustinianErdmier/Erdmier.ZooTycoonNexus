# About Dialogue — Design Spec

> **Milestone:** 15 (Epic 1 — MVP)
> **Date:** 2026-10-25
> **Author:** Justinian
> **Status:** Approved for implementation (calls C1–C10 accepted)
> **Authoritative spec:** [1-zoo-tycoon-nexus-master-design.md](../1-zoo-tycoon-nexus-master-design.md) §9.1, §9.9, §9.10, §13.5
> **Deferred work taken:** DW-23, version half only (the `V1.0.0` literal repeated in `MainWindow.axaml`)
> **Design reference:** `Docs/UserInterfaceDesign/HiFiPrototype/src/dialogs.jsx` (`AboutDialog`, header and footer only), `Docs/UserInterfaceDesign/conventions.md` §4–§5
> **Branch:** `feat/milestone-15-about-dialogue`

---

## 1. Goal

Make **Help → About** open the §9.9 About dialogue. It is a plain Windows 95-style box that tells the player which version and build they are running and what the launcher
is built with. At the same time, take the version **out of the hand-typed XAML**. The version is set once, in the build settings, and read back at runtime, so the About box
and the status bar can never disagree with the build or with each other.

## 2. Decisions (maintainer)

| # | Question                  | Decision                                                                                                                                         |
|---|---------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------|
| 1 | What the dialogue is for  | **Version and credits only**, as §9.9 describes it. The prototype's personal note, Ko-fi button and signature are **out**.                         |
| 2 | Logo                      | `Assets/BrandIcons/ZTL_Primary-Icon.png` (already in the repo). No new artwork in this milestone.                                                 |
| 3 | Where the version lives   | **Worked out automatically from the build settings**: never typed into a view or view model.                                                      |
| 4 | Title bar                 | **Drops the version.** The main window title becomes `  Zoo Tycoon Nexus` (two leading spaces, conventions §4).                                  |
| 5 | Status bar                | **Keeps its version cell** in both the booting and booted bars, now bound to the build-stamped version instead of the `V1.0.0` literal.         |

## 3. Non-goals and anti-goals

- No personal message, Ko-fi link, signature, hyperlinks or "check for updates".
- No new brand artwork and no rename of the `ZTL_*` assets (that stays a milestone 14 follow-up).
- No Help → Contents work; it stays the inert stub §9.10 describes.
- **Anti-goal:** any version string that a person has to remember to update in more than one place. If bumping the release means editing a view, a test literal *and* the
  props file, this milestone has failed even if the dialogue looks right.
- **Anti-goal:** the dialogue showing a version or date that is not the build's own: a placeholder `1.0.0.0`, a default `0001.01.01`, or a date that is today's rather than the
  build's.

## 4. The design

### 4.1 Build settings: the single source

`Directory.Build.props` gains, in its existing `PropertyGroup`:

- `<Version>1.0.0</Version>`: the one place the release number is set. Every project inherits it, which is harmless and keeps the assemblies consistent.
- `<Authors>Justinian Erdmier</Authors>` and `<Copyright>© 2026 Justinian Erdmier</Copyright>`: the SDK stamps both into the assembly attributes.

The **build date** is stamped by the Desktop project only. Its `.csproj` adds an `AssemblyMetadata` item with key `BuildDateUtc`, whose value is the UTC date of the build
formatted `yyyy-MM-dd` (an MSBuild property function over `System.DateTime.UtcNow`). The value is date-only on purpose. The repo builds with `<Deterministic>true</Deterministic>`,
and a date-only stamp changes the output at most once a day, so incremental builds keep working within a day.

### 4.2 Reading it back: `IProductInfo`

A small read-only abstraction in `Erdmier.ZooTycoonNexus.Desktop.Composition`, beside `IDialogService`:

- `IProductInfo`, with `string Version` (three-part, e.g. `1.0.0`), `DateOnly BuildDateUtc` and `string Copyright`.
- `AssemblyProductInfo`, the real implementation. It reads the Desktop assembly's `AssemblyInformationalVersionAttribute`, `AssemblyMetadataAttribute("BuildDateUtc")` and
  `AssemblyCopyrightAttribute` once, at construction.
  - **Version:** the informational version with any `+<metadata>` suffix removed. The .NET SDK appends `+<commit sha>` when source-revision information is available, and that
    must not reach the UI. A pre-release label such as `-beta.1` is kept.
  - **Missing attributes** do not throw. A missing or unparsable build date, or a missing copyright, falls back to a visibly-placeholder value (version `0.0.0`, date
    `DateOnly.MinValue`, copyright empty) and logs a warning. The tests in §6 make sure a real build never takes that path.
- Registered as a **singleton** in `DesktopServiceCollectionExtensions`.

`IProductInfo` is a Desktop concern: it describes the shipped executable, not the domain. It stays out of Application and Infrastructure, so it needs no new architecture
rules.

### 4.3 Main window

- `MainWindow.axaml` line 10: `Title="  Zoo Tycoon Nexus"`, a plain literal again with no binding.
- Both status-bar version cells bind to a new `MainWindowViewModel.VersionText` (`$"V{productInfo.Version}"`, e.g. `V1.0.0`), replacing the two `<TextBlock>V1.0.0</TextBlock>`
  literals. The cell proportions (boot 90/10, booted 70/20/10) do not change.
- `MainWindowViewModel` takes `IProductInfo` in its constructor. It has no designer constructor (it is the documented pair-rule exception), so there is no null object to
  add. Instead, every test that constructs it directly passes a substitute: `ViewLoadTests.CreateMainWindowViewModel`, `MainWindowViewModelGuardTests`,
  `MainWindowViewModelOptionsTests`, `MainWindowViewModelStatusTests`, `MainWindowViewModelStyleMenuTests` and `Views/MainWindowStyleMenuTests`.
- `MainWindow.axaml` must stay within its 100-line architecture limit. The edits replace content, so the line count does not grow.

### 4.4 Help menu

- `<MenuItem Header="About" />` gains `Command="{Binding OpenAboutCommand}"`. The menu line keeps the maintainer's one-line `Contents / Separator / About` formatting
  (milestone 12 ruling R11).
- `MainWindowViewModel.OpenAboutAsync` is a `[RelayCommand]` that calls `_dialogs.ShowAboutAsync()`. It is enabled in every state after boot, like `Options…`, and the menu
  is hidden while booting anyway. There is no access key, so §9.10's "only Options has an access key" stays true.
- `IDialogService.ShowAboutAsync()` returns `Task`. `AvaloniaDialogService` resolves `AboutDialogViewModel` from DI and shows `AboutDialogView` modally over the owner, following
  `ShowOptionsAsync`, minus the `InitialiseAsync` step (nothing loads). The three `file`-scoped `IDialogService` null objects (`PlayViewModel`, `IniConfigTabViewModel`,
  `IniEditorViewModel`) and any test fakes gain the method as a no-op.

### 4.5 The dialogue

A view pair, `ViewModels/Dialogs/AboutDialogViewModel.cs` + `Views/Dialogs/AboutDialogView.axaml`, built in the same mould as `ErrorMessageView`: `ClassicWindow`, no
resize, minimise/restore caption visuals collapsed, `ShowInTaskbar="False"`, `WindowStartupLocation="CenterOwner"`, `SizeToContent="Height"`.

- **Window:** title `  About Zoo Tycoon Nexus`. Title-bar icon `ZTL_Title-Icon.png`, as the main window and the error message box use. Width **430**, the prototype's.
- **Header row:** `ZTL_Primary-Icon.png` drawn at **64 px**, top-aligned on the left. Beside it, a stack of three lines:
  1. `Zoo Tycoon Nexus`: `FontSizeAboutTitle` (13) and `FontWeightStrong`. This is the first use of the `FontSizeAboutTitle` token, which milestone 11.1 created for exactly this.
  2. `Version 1.0.0 (build 2026.10.25)`, built from `IProductInfo` (date formatted `yyyy.MM.dd`, invariant culture).
  3. `A native Windows launcher and configuration tool for Zoo Tycoon (2001).`, muted (`ForegroundMuted`) and wrapping.
- **Separator:** the etched horizontal rule the other dialogues use.
- **Attribution:** `Built with .NET 10, Avalonia 11.3, Classic.Avalonia, SQLite via EF Core and Serilog.`, then `© 2026 Justinian Erdmier` (from `IProductInfo.Copyright`)
  on its own line. Both wrap.
- **Footer:** one `OK` button, right-aligned per conventions §5, `IsDefault` **and** `IsCancel`, so Enter and Esc both close it, as does the title-bar close.

The view model exposes `ProductName`, `VersionLine`, `Description`, `Attribution` and `Copyright` as read-only strings, computed once from the injected `IProductInfo`. It has no
commands. `OK` closes through a code-behind click handler, as `ErrorMessageView` does. Its designer constructor uses a `file`-scoped `IProductInfo` null object
(`1.0.0`, 25 October 2026).

The library list is **fixed copy**, naming the versions as major/minor families. It is not read from the package references, so a patch bump of any package needs no edit
here. A move to `.NET 11` or `Avalonia 12` is a one-line copy change, and the plan's review checklist should catch it.

## 5. Documents to amend in this milestone

- **SDD §9.1:** the title bar becomes `Zoo Tycoon Nexus` (no version). The status-bar bullet says the version label comes from the build (`IProductInfo`).
- **SDD §9.9:** name the view pair (`AboutDialogViewModel` / `AboutDialogView`, as §9.8 does for Options), the logo asset, the `Version`/`build` sources, the exact copy
  above, and the `OK` = default + cancel behaviour.
- **SDD §9.10:** the About row's note changes from "Stub for now" to "Opens the §9.9 dialogue", with command `OpenAboutCommand`.
- **SDD §13.5 (deferred work register):** DW-23 narrows to the status messages and the repeated "Cannot Reload zoo.ini" title, now that the version literal is gone.
- **SDD document control:** a new 1.19 row for milestone 15.
- **`conventions.md` §4 icon table:** the About row becomes `ZTL_Title-Icon.png` (title bar) / `ZTL_Primary-Icon.png` (body, 64 px), with the *(not built)* note removed.
- **Manual test plan:** add an About case (opens from Help, shows the build's version and today's build date on a fresh build, closes with OK / Enter / Esc / ✕, status bar
  shows the same version).
- **`.slnx`:** register this spec, the plan and the ledger under their `/Docs/…` folders in milestone order.

## 6. Testing

- **`AssemblyProductInfo` against the real Desktop assembly:** the version matches `Directory.Build.props` (the test reads the props file's `<Version>`, so the test never
  carries its own copy of the number). There is no `+` in the version. The build date is not `DateOnly.MinValue` and is within a day of the test run. The copyright starts
  with `©`.
- **Version suffix stripping:** `1.2.3+abc123` → `1.2.3`; `1.2.3-beta.1+abc` → `1.2.3-beta.1`; `1.2.3` unchanged. This is a pure function on `AssemblyProductInfo`, tested
  directly.
- **`AboutDialogViewModel`:** with a fake `IProductInfo` (`2.4.6`, 2027-01-09), `VersionLine` is `Version 2.4.6 (build 2027.01.09)` and `Copyright` passes through.
- **`MainWindowViewModel`:** `VersionText` is `V` + the fake's version; `OpenAboutCommand` calls `ShowAboutAsync` once.
- **`ProductNameTests`:** the main-window title assertion becomes `"  Zoo Tycoon Nexus"`. A new case asserts `AboutDialogView`'s title is `"  About Zoo Tycoon Nexus"`.
- **`ViewLoadTests`** picks up the new view through its existing sweep, and the view-pair architecture test checks the new pair.
- **Run the app** (CLAUDE.md *Test the running app like a user*): open Help → About and compare against the prototype header and footer. Check the status bar shows the same
  version, then close the app.

## 7. Calls made by the author

These go in the ledger as rulings when the plan runs. Each is cheap to reverse.

- **C1:** starting version `1.0.0`, matching the current literal, so nothing visible changes yet.
- **C2:** the copyright is a fixed `© 2026 Justinian Erdmier` in the props file, not computed from the build year. A copyright year is the year of first publication, so it should
  not roll over by itself.
- **C3:** the build date is date-only and UTC (the repo's UTC rule). It is shown as `yyyy.MM.dd`, the prototype's format, with no localisation, because it is an identifier, not
  a human date.
- **C4:** the status-bar cell shows the three-part version only, with no build date.
- **C5:** the informational version, minus `+metadata`, is the displayed version, so a future pre-release label appears without code changes.
- **C6:** the title-bar icon is `ZTL_Title-Icon.png`, the one every other branded window uses.
- **C7:** the attribution and copyright copy as written in §4.5. The prototype's FamFamFam Silk icon credit is dropped, because the launcher draws
  Windows 95 icons, not Silk. The prototype's large centred main logo (`ZTL_Main-Logo.png`) under the separator is dropped too:
  decision 1 keeps the box to version and credits, and the 64 px icon already carries the brand.
- **C9:** `OK` has no access key (the prototype marks `O`). As both default and cancel button, Enter and Esc already reach it, and §9.10 keeps access keys rare.
- **C10:** `AssemblyProductInfo` takes an `ILogger<AssemblyProductInfo>` for its fallback warning. The real-assembly test allows the build date to be up to one day before the
  test run (UTC), so a build just before midnight still passes.
- **C8:** `IProductInfo` lives in `Desktop.Composition` and is not an Application port, because nothing below the Desktop layer needs it.

## 8. Left to the builder

- The exact MSBuild expression for the build-date stamp, provided it yields `yyyy-MM-dd` in UTC.
- Margins and spacing inside the dialogue, using the existing spacing tokens and the prototype as the visual target.
- How the props-file version is located from the test (walk up to the repo root or use an MSBuild-injected path), provided the number is never duplicated in test code.

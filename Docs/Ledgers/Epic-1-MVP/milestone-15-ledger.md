# Milestone 15 — About Dialogue: Ledger

**Date:** 2026-10-25

**Plan:** [`milestone-15-about-dialogue.md`](../../Plans/Epic-1-MVP/milestone-15-about-dialogue.md) · **Spec:**
[`milestone-15-about-dialogue-design.md`](../../Specs/Epic-1-MVP/milestone-15-about-dialogue-design.md) · **Branch:** `feat/milestone-15-about-dialogue`

Help → About now opens the §9.9 About dialogue. It shows the launcher's logo, name, `Version <n.n.n> (build <yyyy.MM.dd>)`, a one-line description, the library credits
and the copyright, with a single OK button.

The release number lives in one place, `<Version>` in `Directory.Build.props`. The Desktop assembly also carries the UTC build date and the copyright. `IProductInfo` reads
them back at runtime. The main window title no longer shows a version, and both status-bar version cells bind to the build's version.

Task 1 built the product information. Task 2 built the dialogue, Task 3 wired the Help menu and the main window, Task 4 updated the documents, and Task 5 checked the result in
the running app.

## Rulings taken without the maintainer

The spec's calls C1–C10 (spec §7) were accepted with the spec and were applied as written. The rulings below were taken while the plan ran.

- **R1 (pre-flight, plan defect):** Task 2's `LongCopy_WrapsWithinWindowWidth` asserted that the window was still 430 px wide. The window's `Width` is fixed, so that holds even
  when nothing wraps. The test now finds the copyright `TextBlock` holding a 300-character fake and asserts `TextWrapping.Wrap` and `Bounds.Height > FontSize * 2`, which proves
  the text broke onto at least two lines. Cost if wrong: none; the test is stricter.
- **R2 (pre-flight, plan defect):** Task 2's `Logo_IsPrimaryIconAt64` asked for the image's source path. An `Image.Source` set in XAML is a decoded `Bitmap` with no path, so the
  test asserts a non-null source at 64 × 64 instead. The asset name was checked in review and in the running app. Cost if wrong: a wrong logo file could pass the test, but it
  would still show on screen.
- **R3 (Task 3):** the plan's `1.0.0` search was meant to find only the designer constructor. It also finds three XML doc comments in `IProductInfo` and `AssemblyProductInfo`
  that use `1.0.0` / `1.0.0+65e5c3d` as format examples. They describe the format, not the app's version, so they stay. Cost if wrong: three example strings.
- **R4 (Task 4):** the task review's three documentation minors were fixed in the same round as its Important finding, rather than deferred. These were the pixel-crisp scope in
  conventions §2, a missing blank line in the manual test plan, and a "done" marker on the maintainer's version note. CLAUDE.md asks for polish to be fixed before the PR. Cost
  if wrong: none.
- **R5 (final review):** of the deferred per-task minors, the reviewer's "fix now" set was fixed in the final wave. That set was named arguments on test literals, class
  summaries, an exact warning count, a distinct copyright fake and a final newline. The rest stay as they are; see *Outstanding items*. Cost if wrong: test-style polish only.
- **R6 (final review):** the build-date stamp now uses `UtcNow.ToString('s').Substring(0, 10)` instead of `ToString('yyyy-MM-dd')`. Under MSBuild's current culture, a Thai
  or Saudi build machine would have stamped a Buddhist or Hijri year. Cost if wrong: none; the output is the same on a Gregorian machine.
- **R7 (final re-review):** the fix wave's re-review found no Critical or Important issues, so the waves stop there, as CLAUDE.md directs. One Minor is left: in
  `DesktopAssembly_BuildDateIsRecent`, `ShouldNotBe(DateOnly.MinValue)` and `AddDays(-1)` still pass unnamed literals. Cost if wrong: two unnamed arguments.

## Defects found in the plan

- Task 2's wrap and logo tests could not fail as written (R1, R2).
- The plan's `1.0.0` search did not allow for doc-comment format examples (R3).
- Spec §5 aimed the DW-23 amendment at §13.2. The deferred-work register is §13.5. This was caught by the builder check before the plan was written.

## Task outcomes

- **Task 1** (`ddff678`): `Directory.Build.props` gains `Version` 1.0.0, `Authors` and `Copyright`. The Desktop `.csproj` stamps `BuildDateUtc`. `IProductInfo` and
  `AssemblyProductInfo` are in `Desktop.Composition`. The singleton strips `+<sha>`, and falls back to `0.0.0` / `DateOnly.MinValue` / empty with a warning.
- **Task 2** (`8964f39`): the `AboutDialogViewModel` / `AboutDialogView` pair, and a new `Image.IconLogo` style for the 64 px logo.
- **Task 3** (`4bf1531`): `IDialogService.ShowAboutAsync`, `OpenAboutCommand` and `VersionText`. The title is `  Zoo Tycoon Nexus`, and both status cells bind the version.
  At this point every test project passed and the full build had 0 warnings.
- **Task 4** (`2710dd1`, fix `58ed199`): SDD revision 1.19 (§9.1, §9.9, §9.10, §11.4, §13.5), conventions §2/§4 and a manual About test case.
- **Task 5** (controller): the built Desktop exe was started with `Start-Process` and driven through user32.
  - The title bar reads `Zoo Tycoon Nexus` with no version. The booting and booted status bars both show `V1.0.0`.
  - Help → About opens a 430 px box centred on the main window, with the logo, `Version 1.0.0 (build 2026.10.26)`, the description, an etched separator, the attribution,
    the copyright and OK.
  - OK, Enter, Esc and ✕ each close it.
  - The build date reads the UTC day, which was already 26 October while it was still the 25th locally (C3).
  - The launcher was closed afterwards.

## Outstanding items

**Revisit if the test suite is tidied:**

- `AssemblyProductInfoTests` keeps local `using System.Reflection.Emit;` / `using System.Xml.Linq;`, each used once in that one file, as the view tests already do.
- The `IProductInfo` NSubstitute fake (`2.4.6`, 2027-01-09) is built separately in `AboutDialogViewModelTests` and `AboutDialogViewTests`.
- `AboutDialogViewTests.LongCopy_WrapsWithinWindowWidth` still asserts `Bounds.Width == 430`, which proves nothing by itself; the R1 assertions carry the check.

**Revisit if reproducible builds matter:**

- The `BuildDateUtc` stamp changes the Desktop assembly once a UTC day, despite `<Deterministic>true</Deterministic>` (C3). A CI could pin it to the commit date instead.
- `DesktopAssembly_BuildDateIsRecent` fails when the tests run against a Desktop build more than a day old (for example `dotnet test --no-build` the next day; C10).

**Revisit when the brand artwork is renamed (milestone 14 follow-up):**

- The About dialogue uses `ZTL_Primary-Icon.png` and `ZTL_Title-Icon.png`; new art and asset names still read "ZTL".

**Revisit when the library line changes:**

- The attribution names `.NET 10`, `Avalonia 11.3`, `Classic.Avalonia`, `SQLite via EF Core` and `Serilog` as fixed copy (spec §4.5). A major-version move needs a one-line edit.

## Incidents

- **I1:** one task re-review was first dispatched on Sonnet, against CLAUDE.md's rule that every review seat runs on Opus. It was stopped before it reported and was re-run on
  Opus. Cost: none.

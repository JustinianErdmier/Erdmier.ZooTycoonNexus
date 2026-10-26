# Milestone 15 — About Dialogue: Ledger

**Date:** 2026-10-25

**Plan:** [`milestone-15-about-dialogue.md`](../../Plans/Epic-1-MVP/milestone-15-about-dialogue.md) · **Spec:**
[`milestone-15-about-dialogue-design.md`](../../Specs/Epic-1-MVP/milestone-15-about-dialogue-design.md) · **Branch:** `feat/milestone-15-about-dialogue`

*Work in progress: this ledger is written while the plan runs and tidied when it finishes.*

## Rulings taken without the maintainer

- **R1 (pre-flight, plan defect):** Task 2's `LongCopy_WrapsWithinWindowWidth` asserts the window is still 430 px wide. The window's `Width` is fixed, so that holds even when
  nothing wraps, and the test proves nothing. It now finds the copyright `TextBlock` and asserts `TextWrapping.Wrap` and a laid-out height of more than two line heights (a
  300-character line cannot fit on one line at 430 px). Cost if wrong: none; the test is stricter.
- **R2 (pre-flight, plan defect):** Task 2's `Logo_IsPrimaryIconAt64` asks for the image source path, but an `Image.Source` set in XAML is a decoded `Bitmap` with no path. The
  test asserts a non-null source at 64 × 64. The asset name is checked in review and in the running app. Cost if wrong: a wrong logo file could pass the test, but would still be
  caught on screen.
- **R3 (Task 3):** the plan's `1.0.0` search should find only the designer constructor. It also finds three XML doc comments in `IProductInfo` and `AssemblyProductInfo`
  that use `1.0.0` / `1.0.0+65e5c3d` as format examples. They describe the format, not the app's version, so they stay. Cost if wrong: three example strings.

## Defects found in the plan

- Task 2's wrap and logo tests could not fail as written (R1, R2).

## Task 5: the app check

The built Desktop exe was started with `Start-Process` and driven through user32. The title bar reads `Zoo Tycoon Nexus` with no version. The booting status bar and the
booted one both show `V1.0.0`. Help → About opens a 430 px box centred on the main window, with the logo, `Version 1.0.0 (build 2026.10.26)`, the description, an etched
separator, the attribution, the copyright and OK. OK, Enter, Esc and ✕ each close it. The build date reads the UTC day, which was already 26 October while it was still the
25th locally (C3). The launcher was closed afterwards.

## Outstanding items

*None yet.*

## Incidents

- **I1:** one task re-review was first dispatched on Sonnet, against CLAUDE.md's rule that every review seat runs on Opus. It was stopped before it reported and re-run on
  Opus. Cost: none.

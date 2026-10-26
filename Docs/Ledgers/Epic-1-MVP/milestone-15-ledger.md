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

## Defects found in the plan

- Task 2's wrap and logo tests could not fail as written (R1, R2).

## Outstanding items

*None yet.*

## Incidents

*None yet.*

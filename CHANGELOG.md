# Changelog — ProShitje WPF

## 2.0.0 — Baseline reset
- Established the WPF project as the only official project line.
- Target remains .NET Framework 4.8, x86.
- Preserved the existing WPF prototype, its UI, and its GitHub Actions workflow.
- Added a version manifest and explicit build/test status rules.
- No new sales, CRUD, or licensing functionality is claimed in this baseline.
- Build status: NOT VERIFIED in this delivery; run GitHub Actions on GitHub.

## 2.0.2
- Fix startup NullReferenceException: guard both SubtotalText and TotalText in UpdateTotals during XAML initialization.
- No database, invoice, or UI behavior changes intended.
- Build/runtime not verified here; verify via GitHub Actions and run the generated EXE.

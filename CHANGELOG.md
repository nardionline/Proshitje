# Changelog

## 2.0.6 - Fix Build Errors
- Added the WPF Controls.Primitives namespace required by UniformGrid.
- Corrected invalid two-argument Thickness constructors to four-argument constructors.
- Build status: not verified locally; check GitHub Actions.


## 2.0.4
- Fixed accidental literal `\n` before `SettingsWindow` that caused CS1056/CS1585.
- No functional changes to invoice logic.
- Build not verified locally; validate with GitHub Actions.

# Changelog — ProShitje WPF

## 2.0.0 — Baseline reset
- Established the WPF project as the only official project line.
- Target remains .NET Framework 4.8, x86.
- Preserved the existing WPF prototype, its UI, and its GitHub Actions workflow.
- Added a version manifest and explicit build/test status rules.
- No new sales, CRUD, or licensing functionality is claimed in this baseline.
- Build status: NOT VERIFIED in this delivery; run GitHub Actions on GitHub.


## 2.0.3 — 2026-10-22
- Sasia mund të shkruhet nga tastiera normale dhe të ndryshohet me tastierë numerike me touch.
- Kontrollim i sasisë dhe rifreskim i totalit pas ndryshimit.
- Ndryshimi i çmimit nga menuja me të djathtën është i mbyllur derisa të jepet autorizim me PIN.
- U shtua dialog fillestar për autorizimin e ndryshimit të çmimit dhe lejen e shikimit të kostos/marzhit.
- Çmimi i blerjes/marzhi nuk janë aktivizuar për shfaqje: fusha përkatëse e MDB duhet identifikuar përpara se të llogaritet fitimi.
- Build-i nuk është verifikuar ende në Windows; kërkohet GitHub Actions.


## 2.0.5 - Fatura Layout
- Moved unit display under the product description in the invoice.
- Removed explicit duplicate Microsoft.WinFX.targets import; Microsoft.CSharp.targets imports framework targets.
- No database schema or save behavior changes.
- Build not verified on Windows.

# ProShitje WPF 2.0.8 - Fix Duplicate Actions

Base: ProShitje WPF 2.0.7 - Fatura Popup & Maximized.

Changes: removed the duplicate local declaration of `actions` in `TouchKeypadWindow`, which caused CS0128. The existing `actions` UniformGrid remains docked at the bottom and receives the Cancel/Confirm buttons.

Build status: NOT VERIFIED on Windows/MSBuild. Run GitHub Actions before use.

# ProShitje — Version Manifest

- Version: `2.0.0`
- Project type: C# WPF
- Target: .NET Framework 4.8, x86
- Database target: Microsoft Access MDB (Jet OLEDB 4.0)
- Base source: `ProShitjeWPF_Prototype_2.0.zip`
- Build status: `NOT VERIFIED` — run GitHub Actions after upload
- Functional status: `PROTOTYPE` — not production-ready

## Rules
- This repository is WPF-only. Do not add Windows Forms project files.
- Do not overwrite old release ZIPs.
- One focused change per version.
- Update `CHANGELOG.md` and this file with every change.
- Mark BUILD PASS only when the corresponding GitHub Actions run succeeds.
- Mark a feature TESTED only after testing it in the actual Windows app.

# ProShitje WPF — GitHub source package

Modern WPF POS interface for Windows, targeting .NET Framework 4.8 x86 and Microsoft Access 2003 (Jet OLEDB 4.0).

## Included in this source package
- Modern WPF sales screen with two-column product cards.
- Invoice grid with product, unit, quantity, price and line total.
- Product search from the configured Access MDB (`Artikulli` table).
- Demo catalog when no database is configured.
- Settings dialog for the MDB path/password and fiscal TXT output folder.
- TXT export prototype and Windows print dialog.
- GitHub Actions workflow to build the Windows x86 package.

## Important — current implementation status
This repository is a **development prototype, not a production-ready cash register**.

- F6 now attempts to save a sales invoice to `Fature Hyrje-Dalje` and its item rows to `Hyrje-Dalje`, then updates `Artikulli.[Sasia Stock]` in the same Access transaction. It blocks sales when stock is insufficient. Test this only on a copy of the MDB; field mappings and business rules still require validation against actual application behavior.
- Client and supplier maintenance, dynamic rewards configuration, and a persistent points ledger are **not yet integrated into the WPF sales screen**.
- The existing `Klienti.Piketegrumbulluara` field is present in the supplied schema, but a reliable points ledger and market-configurable conversion rules need to be implemented and tested before use.
- The fiscal TXT output only confirms that a text file was created; it does not confirm fiscal-device acceptance.
- Printing uses the standard Windows print dialog and has not been validated on a thermal printer.
- Purchase-entry screens, manual warehouse movements, stock adjustments/transfers, customer/supplier management and reports are not yet integrated. The invoice numbering, VAT handling, discount allocation, cancellation and transaction behavior must be verified against a copy of the MDB before production use.

## Upload to GitHub
1. Extract this ZIP into a new folder.
2. Create/open the intended GitHub repository.
3. Upload the **contents** of the extracted folder (not the ZIP itself), including `.github/workflows/build.yml`.
4. Commit to `main` or `master`.
5. Open **Actions** and select `Build ProShitje WPF x86`.
6. Download artifact `ProShitjeWpf-Windows-x86` after the workflow succeeds.

## Build and runtime requirements
- Windows with Visual Studio Build Tools / MSBuild and .NET Framework 4.8 targeting pack.
- The MDB connection uses Jet OLEDB 4.0 and the application is configured for x86. The target Windows machine must have a compatible Jet provider installed.
- Do not commit a real MDB file, database password, or customer data. Keep a separate backup and use a test copy during development.

## Configure the MDB
Run the built application, open **Cilësimet**, choose a test copy of the `.mdb`, enter its password if required, click **Testo MDB**, then save. Do not test write operations against the production database.

## Validation note
The build is delegated to GitHub Actions on Windows. This source package has not been compiled in the current Linux workspace, so a successful Windows Actions run is still required to validate the build.

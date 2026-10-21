# ProShitje 1.3 - starter project

Ky është projekti fillestar për ndërtim përmes GitHub Actions. Është një prototip, jo zëvendësim i gatshëm për programin aktual.

## Çfarë përmban
- Aplikacion Windows WinForms, 32-bit, i ndërtuar me .NET Framework 4.8.
- Hyrje e thjeshtë me role Administrator/Shitës (prototip; autentikimi duhet forcuar para përdorimit real).
- Kërkim artikujsh në tabelën `Artikulli` sipas fushës `Artikulli` ose `Barcodi`.
- Gjenerim i skedarit AED TXT nga artikujt e zgjedhur.

## GitHub Actions
Ngarko përmbajtjen e kësaj dosjeje në root të repository-t. Workflow `.github/workflows/build.yml` kompajlon projektin në Windows dhe publikon ZIP-in si artifact.

## Para testimit
1. Vendos një KOPJE të `marketdb.mdb` në të njëjtën dosje me `ProShitje13.exe`.
2. Kontrollo në Windows që `Microsoft.Jet.OLEDB.4.0` është i disponueshëm për proces 32-bit.
3. Nëse MDB-ja është e mbrojtur me fjalëkalim, aplikacioni kërkon fjalëkalimin në dritaren e lidhjes. Mos e ruaj fjalëkalimin në repository.
4. Provo fillimisht me kopje të databazës dhe kontrollo skedarin AED me programin fiskal.

## Shënime të rëndësishme
- Formati AED bazohet në shembujt e dhënë: `CLEAR`, `KEY REG`, një rresht `SALE` për artikull, `SUBTOT`, `CLOSE TENDER=1`.
- Vlera e `DPT`, kuptimi i vlerave `TENDER`, formatet për kthime/anulime, dhe konfirmimi i printimit duhet të verifikohen me programin fiskal.
- Ky prototip NUK regjistron ende shitjet në tabelat e faturave dhe lëvizjeve të magazinës. Kjo duhet implementuar pasi të konfirmohet logjika ekzistuese e databazës.
- Mos e përdor ende si sistem real shitjesh pa testim dhe validim.

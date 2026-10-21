# ProShitje 1.3 — zhvillim mbi repository-n ekzistues

## Përmirësime në këtë paketë
- Ndërfaqe Windows Forms me menu anësore dhe dashboard.
- Kërkim artikujsh me vonesë të shkurtër pas shkrimit (debounce), pa kërkuar pas çdo tastiere; kërkon sipas barkodit, kodit dhe emërtimit.
- Kërkimi kryhet në punë të veçantë (async-style) me lidhje të veçantë Access, që UI të mbetet më reagues.
- Barkodi/kodi i saktë renditet përpara rezultateve të përafërta; në shitje, përputhja e vetme e saktë e barkodit shtohet automatikisht në shportë.
- Ekran shitjeje me rezultate, shportë, klient (fusha UI për momentin), sasi, çmim, total dhe eksport AED TXT.
- Konfigurim MDB i ruajtur në `%LOCALAPPDATA%\\ProShitje13\\config.xml`.
- Workflow i GitHub Actions për build Windows x86 me NuGet restore.

## Kufizime që duhen ditur
Ky është version zhvillimi, jo produkt i përfunduar për arkë reale.
- Shitjet ende nuk ruhen në tabelat `[Fature Hyrje-Dalje]` dhe `[Hyrje-Dalje]`.
- Lista e klientëve nuk është ende e lidhur me tabelën `Klienti`; zgjedhja e klientit është vetëm ndërfaqe fillestare.
- Modulet e furnitorëve, hyrje/daljeve të magazinës, detyrimeve, recetave, skadencave dhe raporteve kërkojnë implementim.
- Eksporti AED TXT nuk garanton se fatura është pranuar ose fiskalizuar.
- Seriali vjetor offline nuk është ende licencim i sigurt; `LicenseService.cs` ka sekret demonstrues placeholder. Mos e përdor për shitje/licencim real.
- Fjalëkalimi MDB ruhet aktualisht në konfigurim XML; para përdorimit real duhet ruajtje e mbrojtur me Windows DPAPI.
- Kërkimi automatik varet nga emrat/fushat e raportuara në MDB; testo mbi kopje të databazës.

## Databaza
Nuk fshihet dhe nuk riemërtohet asnjë fushë ekzistuese. Puno me kopje rezervë të `marketdb.mdb`. Jet OLEDB 4.0 kërkon proces 32-bit dhe driver-in përkatës në Windows.

## Build
Ngarko skedarët në root të repository-t. GitHub Actions → `Build ProShitje 1.3` → `Run workflow`. Shkarko artifact `ProShitje13-Windows-x86`.

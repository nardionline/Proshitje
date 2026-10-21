# ProShitje 1.3 — baza e zgjeruar e zhvillimit

## Përfshirë në këtë paketë
- Ndërfaqe WinForms me sidebar dhe dashboard të ridizenjuar.
- Ekran konfigurimi për MDB, fjalëkalim, testim lidhjeje dhe ruajtje te `%LOCALAPPDATA%\ProShitje13\config.xml`.
- Ndarje fillestare për konfigurim, lidhje me Access dhe licencim.
- Kërkim artikujsh nga `Artikulli` dhe gjenerim i formatit AED TXT të dhënë në kërkesat.
- Workflow GitHub Actions për build Windows x86.

## Kufijtë e këtij versioni
Ky është version zhvillimi, jo produkt i përfunduar për arkë reale. Modulet e magazinës, klientëve/furnitorëve, detyrimeve, recetave, skadencave, ruajtjes së shitjeve në MDB, autentikimit të roleve dhe raporteve janë të vendosura në navigim, por kërkojnë implementim dhe testim. Eksporti AED TXT nuk provon që fatura është pranuar nga fiskalizuesi.

## Konfigurimi
Skedari i konfigurimit ruhet jashtë repository-t te `%LOCALAPPDATA%\ProShitje13\config.xml`. Ai përmban fjalëkalimin e MDB-së në tekst të lexueshëm; përpara përdorimit real duhet të zëvendësohet me ruajtje të mbrojtur (p.sh. Windows DPAPI).

## Licenca
Kontrolli i serialit është vetëm skelet demonstrues. Sekreti në `LicenseService.cs` është placeholder dhe **nuk duhet përdorur për licencim real**; një serial i sigurt kërkon gjenerues privat dhe verifikim të projektuar siç duhet. Butoni i testimit lejon hapjen pa licencë.

## Databaza
Nuk fshihet ose ndryshohet asnjë fushë e databazës ekzistuese nga ky projekt. Testo gjithmonë me kopje rezervë të `marketdb.mdb`. Jet OLEDB 4.0 kërkon proces 32-bit dhe përputhshmëri me instalimin e Windows.

## Ndërtimi
Ngarko përmbajtjen e ZIP-it në repository GitHub dhe hap Actions. Shkarko artifact-in `ProShitje13-Windows-x86`.

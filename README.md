# ProShitje 1.3 — Windows x86

Ky është një version i avancuar i ndërfaqes dhe bazës së projektit për Windows Forms / .NET Framework 4.8, i projektuar për MDB Access 2003 me Jet OLEDB 4.0.

## Përfshihet në këtë version
- Ndërfaqe me menu anësore, dashboard dhe pamje të unifikuara.
- Konfigurim i databazës `.mdb`, fjalëkalim, testim lidhjeje dhe ruajtje në profilin lokal të Windows.
- Kërkim artikujsh sipas emërtimit, barkodit dhe kodit; kërkimi bëhet në sfond.
- Shportë provizore shitjeje dhe eksport i përmbajtjes në AED TXT.
- Pamje informative vetëm-lexim për klientët, furnitorët, faturat, hyrjet/daljet dhe artikujt me datë skadence.
- Workflow GitHub Actions për build x86 dhe paketim ZIP.

## Kufizime që duhen zgjidhur para përdorimit real
- Shitja nuk ruhet ende në tabelat `Fature Hyrje-Dalje` dhe `Hyrje-Dalje`; pagesa nuk regjistrohet.
- Eksporti AED TXT është vetëm gjenerim skedari dhe duhet provuar me pajisjen/protokollin real.
- Pamjet e listave janë vetëm-lexim; formularët e krijimit/redaktimit dhe lëvizjet e magazinës nuk janë aktivizuar.
- Licencimi vjetor dhe gjenerimi i serialeve nuk janë implementuar si mekanizëm sigurie. Mos e trajtoni butonin e testimit si licencë komerciale.
- Dashboard-i është vizual; treguesit financiarë nuk duhen konsideruar raporte të verifikuara.
- Konfigurimi ruan aktualisht fjalëkalimin e MDB në XML lokal. Për publikim komercial duhet mbrojtur/kriptografuar.

## Build në GitHub
1. Ngarko përmbajtjen e ZIP-it në rrënjën e repository-t (ku ndodhet `ProShitje13.csproj`).
2. Commit/push në `main` ose `master`, ose nis workflow manualisht te Actions.
3. Shkarko artifact-in `ProShitje13-Windows-x86`.
4. Provoje në Windows me Jet OLEDB 4.0 dhe një kopje rezervë të MDB-së.

## Siguria e të dhënave
Ky version nuk bën shkrime në databazën ekzistuese nga modulet informative. Përpara çdo implementimi që regjistron fatura ose lëvizje, krijo kopje rezervë dhe verifiko lidhjet/master-detail në MDB-në reale. Fushat ekzistuese nuk duhet të fshihen, riemërtohen ose të ndryshohet tipi i tyre pa miratim.


## Ndryshime në ekranin e arkës
- Fusha e kërkimit është më e gjerë dhe tregon qartë përdorimin me skaner/ENTER.
- Shporta ka kolona të lexueshme, lartësi më të madhe rreshtash dhe total të dukshëm.
- Janë shtuar butona të mëdhenj për F5 Fiskale, F6 Ruaj dhe F7 Printo, të përshtatshëm për touchscreen.
- Tastet F5/F6/F7 dhe butonat shfaqin qartë kur integrimi real nuk është konfiguruar. Nuk simulohet një fiskalizim, ruajtje apo printim i suksesshëm.
- Ky është ende version për testim të ndërfaqes: ruajtja e faturës në MDB dhe printimi/fiskalizimi real duhen implementuar e verifikuar para përdorimit në shitje.


### Fix revizion 5
- U hoq plotësisht `SplitContainer` nga ekrani i shitjes, sepse ai po shkaktonte përjashtimin `SplitterDistance` gjatë inicializimit.
- Lista e artikujve dhe shporta tani organizohen me `TableLayoutPanel` me përqindje 53/47, pa `SplitterDistance` dhe pa madhësi minimale të paneleve.

# ProShitje 2.0.0 — WPF Baseline

Ky është versioni bazë zyrtar për vazhdimin e ProShitje. Projekti është **C# WPF**, jo Windows Forms.

## Teknologjia
- C# WPF
- .NET Framework 4.8
- Platforma x86
- Microsoft Access MDB (Jet OLEDB 4.0)

## Gjendja e këtij versioni
Ky është një prototip bazë për rikthimin e kontrollit të versioneve. Ndërfaqja ekzistuese është ruajtur. Nuk pretendohet se ky version është gati për përdorim në biznes ose se build-i është kaluar; duhet verifikuar në GitHub Actions.

## Përfshihet
- Ndërfaqja WPF ekzistuese e shitjes.
- Katalogu/kartat e produkteve dhe paneli i faturës nga prototipi.
- Cilësimet ekzistuese për databazën MDB.
- Workflow i GitHub Actions për build në Windows dhe krijim artifact.
- `VERSION.md` dhe `CHANGELOG.md` për historikun e projektit.

## Si ta ngarkosh në GitHub
1. Shkarko ZIP-in dhe çpaketoje.
2. Ngarko **përmbajtjen** e dosjes në rrënjën e repository-t ekzistues (jo dosjen mbështjellëse).
3. Sigurohu që `ProShitje.Wpf.csproj` është në rrënjën e repository-t.
4. Commit/push në `main` ose `master`.
5. Hape **Actions** dhe ekzekuto `Build ProShitje WPF x86`.
6. Nëse dështon, shkarko log-un e plotë dhe dërgoje këtu. Nëse kalon, shkarko artifact `ProShitjeWpf-Windows-x86`.

## Rregulla versionimi
- Vetëm WPF në këtë linjë.
- Një ndryshim kryesor për çdo version.
- Mos mbishkruaj ZIP-et e mëparshme.
- Përditëso `VERSION.md` dhe `CHANGELOG.md` për çdo ndryshim.
- Mos e shëno një build si të suksesshëm pa rezultat real nga Actions.

## Kufizime të njohura
- Ky prototip nuk është ende sistem i plotë faturimi.
- Ruajtja reale e faturave në tabelat e Access-it, CRUD i plotë për artikuj/klientë/furnitorë dhe licencimi duhet të shtohen e testohen veçmas.
- Testo vetëm mbi kopje prove të MDB-së.

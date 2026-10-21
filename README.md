# ProShitje 2.0 — WPF Prototype

Projekt i ri WPF për Windows, i synuar për .NET Framework 4.8 x86 dhe Access 2003 (Jet OLEDB 4.0). Ky është prototipi i parë funksional i ndërfaqes; nuk është ende version prodhimi.

## Përfshihet
- Ndërfaqe WPF modernizuar, menu anësore dhe panel produktesh.
- Kartat e artikujve shfaqin emërtimin, barkodin, njësinë dhe çmimin.
- Fatura përfshin kolonat Artikulli, Njësia, Sasia, Çmimi dhe Totali.
- Kërkim në MDB në sfond, me debounce; modalitet demo kur MDB nuk është konfiguruar.
- Cilësime për rrugën e MDB-së, fjalëkalimin dhe dosjen e TXT fiskal.
- F5 krijon TXT sipas modelit AED të diskutuar; F6 ruan një draft CSV lokal; F7 hap dialogun e printimit.
- GitHub Actions ndërton paketën Windows x86.

## E rëndësishme — kufijtë e këtij prototipi
- F6 ruan vetëm një draft CSV testues në `%LOCALAPPDATA%\ProShitje\Drafts`; nuk shkruan ende në tabelat e faturave të Access-it.
- F5 konfirmon vetëm krijimin e skedarit TXT, jo pranimin nga pajisja fiskale.
- F7 përdor dialogun standard të printimit të Windows; printimi termik/fiskal duhet testuar me pajisjen reale.
- Të dhënat demo dhe çmimet demo janë ilustrative. Kur MDB është zgjedhur, katalogu lexohet nga tabela `Artikulli` dhe njësia merret nga fusha `[Njësia e matjes]`.
- Para përdorimit real duhet verifikuar skema e plotë e MDB-së, ruajtja e faturës, llogaritja e TVSH-së/zbritjeve, anulimet, numërimi i faturave dhe integrimi fiskal.
- Access MDB nuk rekomandohet si databazë e përbashkët afatgjatë për shumë kasa. Arkitektura duhet të migrojë më vonë në databazë serveri/API.

## Si testohet
1. Hap Cilësime → zgjidh kopjen testuese të MDB-së.
2. Kliko **Testo MDB** dhe ruaj cilësimet.
3. Kërko artikuj; kartat duhet të lexojnë emërtimin, barkodin, njësinë, çmimin dhe stokun nga Access.
4. Kliko një kartë për ta shtuar në faturë. Njësia shfaqet në kolonën e faturës.
5. Testo F5, F6 dhe F7 vetëm me të dhëna prove.

## Build
GitHub Actions → `Build ProShitje WPF x86` → shkarko artifact `ProShitjeWpf-Windows-x86`.

Përdorimi i MDB-së kërkon Windows x86/Jet OLEDB 4.0 të instaluar. Nuk është verifikuar build-i në këtë mjedis; build-i real kryhet nga GitHub Actions në Windows.

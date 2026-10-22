# ProShitje WPF — kapja e gabimit të nisjes

Kjo paketë bazohet te ZIP-i që ngarkove. Ndryshimi kryesor është që gabimet e papërpunuara gjatë nisjes të shfaqen në dritare dhe të ruhen në log.

## Si ta testosh
1. Ngarko përmbajtjen e këtij ZIP-i në repository-n ekzistues WPF dhe bëj commit.
2. Ekzekuto GitHub Actions. Mos vazhdo nëse build-i dështon; dërgo log-un e build-it.
3. Shkarko artifact-in e ri dhe çpaketoje në një dosje të re, p.sh. `C:\ProShitje-Test`.
4. Hape `ProShitjeWpf.exe`.
5. Nëse nuk hapet, shiko për `ProShitje-startup-error.log` në të njëjtën dosje dhe dërgoje këtu.
6. Nëse del dritare gabimi, fotografoje ose kopjo mesazhin e plotë.

Mos përdor MDB me të dhëna reale për testin. Ky version është vetëm diagnostikues dhe nuk pretendohet si i rregulluar pa log-un e testit.

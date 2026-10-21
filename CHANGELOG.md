# Ndryshimet — versioni afër finales

- Rishikim i kodit burimor dhe rregullim i mungesës së metodës `SearchProductsIndependent`, e cila do të shkaktonte gabim build.
- Kërkimi tani mbështet emërtim, barkod dhe kod artikulli.
- Kërkimet asinkrone më të vjetra nuk mbishkruajnë më rezultatin e kërkimit të fundit.
- Barkodi/kodi unik mund të shtohet në shportë në ekranin e shitjes.
- U shtuan pamje informative vetëm-lexim për klientë, furnitorë, fatura, hyrje/dalje magazine dhe artikuj me skadencë.
- U shtua ekran konfigurimi për databazën, me zgjedhje MDB, testim dhe ruajtje.
- U lidh lista e klientëve me tabelën `Klienti` në ekranin e shitjes.
- U përditësua dokumentimi me funksionet që mungojnë ende për përdorim real.

Ky paketim nuk është certifikuar me build në këtë mjedis (nuk disponohet MSBuild/.NET Framework). Build-i përfundimtar duhet konfirmuar në GitHub Actions dhe testuar në Windows me kopje rezervë të MDB-së.

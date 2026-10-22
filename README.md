# ProShitje Licensing Starter
WPF / .NET Framework 4.8 starter.

- Reads a disk serial via Windows WMI (if exposed by the driver).
- Derives N1 and N2 deterministically from the serial hash.
- Validates S = (N1 * 222) + N2 + 222.
- On valid key, inserts Seriali, Datereg, Dateperfudimi into Access table Serial.

Build with GitHub Actions on Windows. Must be tested with the target MDB before production.
Important: formula K=222 is not secure against deliberate reverse engineering. For commercial release, replace client-side arithmetic with a signed license: private key in admin tool, public key in POS.
The table shown by the user contains no SSD field, so this starter does not store the hardware identifier in the Serial table. It also does not yet block duplicate activations or enforce expiry on every application launch.

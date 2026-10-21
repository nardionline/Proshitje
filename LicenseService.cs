using System;
using System.Security.Cryptography;
using System.Text;
namespace ProShitje13 {
 public static class LicenseService {
  // Aktivizim lokal i nënshkruar. Ndrysho çelësin sekret dhe mbaje jashtë repository-t për produkt real.
  private const string ProductCode="PS13";
  private const string SigningSecret="REPLACE_WITH_PRIVATE_VENDOR_SECRET_BEFORE_RELEASE";
  public static bool Validate(string serial) {
   if(String.IsNullOrWhiteSpace(serial)) return false;
   var parts=serial.Trim().ToUpperInvariant().Split('-');
   if(parts.Length!=3 || parts[0]!=ProductCode) return false;
   string payload=parts[0]+"-"+parts[1];
   using(var sha=new SHA256Managed()) {
    var hash=sha.ComputeHash(Encoding.UTF8.GetBytes(SigningSecret+"|"+payload));
    string token=BitConverter.ToString(hash).Replace("-","").Substring(0,8);
    return String.Equals(token,parts[2],StringComparison.OrdinalIgnoreCase);
   }
  }
  // Për gjenerimin e serialeve përdor një mjet të veçantë të shitësit; mos shpërnda sekretin me aplikacionin.
 }
}
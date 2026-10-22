namespace ProShitje.Wpf {
 public sealed class Product {
  public int Id { get; set; } public string Code { get; set; } public string Barcode { get; set; } public string Name { get; set; } public string Unit { get; set; } public decimal Price { get; set; } public decimal Stock { get; set; }
  public string PriceText => Price.ToString("N2") + " lekë";
  public string BarcodeText => string.IsNullOrWhiteSpace(Barcode) ? Code : Barcode;
  public string UnitText => string.IsNullOrWhiteSpace(Unit) ? "—" : Unit;
 }
}

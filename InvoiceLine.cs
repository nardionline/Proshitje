using System.ComponentModel;
using System.Runtime.CompilerServices;
namespace ProShitje.Wpf {
 public sealed class InvoiceLine : INotifyPropertyChanged {
  private decimal quantity=1m;
  public int ProductId { get; set; } public int Index { get; set; } public string Name { get; set; } public string Unit { get; set; } public decimal Price { get; set; }
  public decimal Quantity { get=>quantity; set { quantity=value<=0?1:value; OnPropertyChanged(); OnPropertyChanged(nameof(LineTotal)); } }
  public decimal LineTotal => decimal.Round(Quantity*Price,2);
  public event PropertyChangedEventHandler PropertyChanged;
  private void OnPropertyChanged([CallerMemberName] string name=null)=>PropertyChanged?.Invoke(this,new PropertyChangedEventArgs(name));
 }
}

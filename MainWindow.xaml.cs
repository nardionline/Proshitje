using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ProShitje.Wpf {
 public partial class MainWindow : Window {
  public ObservableCollection<InvoiceLine> InvoiceLines { get; } = new ObservableCollection<InvoiceLine>();
  private readonly DispatcherTimer searchTimer;
  private List<Product> currentProducts = new List<Product>();
  private DatabaseService database;
  private int searchVersion;
  private string paymentType = "Cash";
  private int invoiceNumber = 125;
  private static readonly CultureInfo Sq = CultureInfo.GetCultureInfo("sq-AL");

  public MainWindow() {
   InitializeComponent(); DataContext=this;
   searchTimer=new DispatcherTimer { Interval=TimeSpan.FromMilliseconds(300) }; searchTimer.Tick+=async (s,e)=>{searchTimer.Stop();await LoadProductsAsync(SearchBox.Text);};
   InvoiceLines.CollectionChanged+=(s,e)=>{RenumberLines();UpdateTotals();};
   UpdateTotals();
   if(!string.IsNullOrWhiteSpace(AppConfig.AccessPath)&&File.Exists(AppConfig.AccessPath)) _=LoadProductsAsync("");
   else ShowDemoProducts();
  }

  private async Task LoadProductsAsync(string term) {
   int requestVersion=++searchVersion;
   if(string.IsNullOrWhiteSpace(AppConfig.AccessPath)||!File.Exists(AppConfig.AccessPath)){ShowDemoProducts(term);ConnectionStatus.Text="●  Modalitet demo — MDB nuk është zgjedhur";ConnectionStatus.Foreground=Brushes.Goldenrod;return;}
   CatalogFooter.Text="Duke kërkuar artikuj…";
   try {
    var service=new DatabaseService(AppConfig.AccessPath,AppConfig.AccessPassword);
    var products=await service.SearchAsync(term);
    if(requestVersion!=searchVersion)return;
    currentProducts=products; RenderProducts(products);
    ConnectionStatus.Text="●  Access MDB e lidhur";ConnectionStatus.Foreground=new SolidColorBrush(Color.FromRgb(10,159,98));
    CatalogFooter.Text=products.Count+" artikuj të shfaqur · kërkimi në MDB";
   } catch(Exception ex) {
    if(requestVersion!=searchVersion)return;
    ConnectionStatus.Text="●  Gabim në MDB";ConnectionStatus.Foreground=Brushes.IndianRed;
    ProductPanel.Children.Clear(); currentProducts=new List<Product>(); CatalogFooter.Text="Kërkimi dështoi: "+ex.Message;
   }
  }

  private void SearchBox_TextChanged(object sender,TextChangedEventArgs e) { if(searchTimer!=null){searchTimer.Stop();searchTimer.Start();} }
  private void ShowDemoProducts(string term="") {
   var demos=new List<Product>{
    new Product{Id=1,Code="ART-001",Barcode="869123456001",Name="Bukë klasike",Unit="copë",Price=50,Stock=24},
    new Product{Id=2,Code="ART-002",Barcode="869123456002",Name="Qumësht 1L",Unit="copë",Price=120,Stock=18},
    new Product{Id=3,Code="ART-003",Barcode="869123456003",Name="Ujë natyral 1.5L",Unit="copë",Price=60,Stock=36},
    new Product{Id=4,Code="ART-004",Barcode="869123456004",Name="Coca Cola 1.5L",Unit="copë",Price=130,Stock=20},
    new Product{Id=5,Code="ART-005",Barcode="869123456005",Name="Lays klasike 90g",Unit="copë",Price=110,Stock=12},
    new Product{Id=6,Code="ART-006",Barcode="869123456006",Name="Sheqer 1kg",Unit="kg",Price=100,Stock=15},
    new Product{Id=7,Code="ART-007",Barcode="869123456007",Name="Miell 1kg",Unit="kg",Price=80,Stock=20},
    new Product{Id=8,Code="ART-008",Barcode="869123456008",Name="Vaj luledielli 1L",Unit="copë",Price=190,Stock=10},
    new Product{Id=9,Code="ART-009",Barcode="869123456009",Name="Oriz 1kg",Unit="kg",Price=120,Stock=15},
    new Product{Id=10,Code="ART-010",Barcode="869123456010",Name="Makarona 500g",Unit="paketë",Price=70,Stock=22},
    new Product{Id=11,Code="ART-011",Barcode="869123456011",Name="Kafe 100g",Unit="paketë",Price=150,Stock=8},
    new Product{Id=12,Code="ART-012",Barcode="869123456012",Name="Biskota 200g",Unit="paketë",Price=90,Stock=14},
    new Product{Id=13,Code="ART-013",Barcode="869123456013",Name="Detergjent 1L",Unit="copë",Price=230,Stock=6},
    new Product{Id=14,Code="ART-014",Barcode="869123456014",Name="Sapun 100g",Unit="copë",Price=60,Stock=30},
    new Product{Id=15,Code="ART-015",Barcode="869123456015",Name="Letër higjienike 4r",Unit="paketë",Price=180,Stock=11},
    new Product{Id=16,Code="ART-016",Barcode="869123456016",Name="Kabllo elektrike",Unit="metër",Price=80,Stock=45}
   };
   var q=(term??"").Trim();if(q.Length>0)demos=demos.Where(p=>(p.Name??"").IndexOf(q,StringComparison.CurrentCultureIgnoreCase)>=0||(p.Barcode??"").Contains(q)||(p.Code??"").IndexOf(q,StringComparison.OrdinalIgnoreCase)>=0).ToList();
   currentProducts=demos;RenderProducts(demos);CatalogFooter.Text="DEMO · "+demos.Count+" artikuj shembull · konfiguroni MDB për të dhëna reale";
  }

  private void RenderProducts(IEnumerable<Product> products) {
   ProductPanel.Children.Clear();
   foreach(var product in products) {
    var card=new Border{Width=146,Height=188,Margin=new Thickness(0,0,10,10),Padding=new Thickness(12),CornerRadius=new CornerRadius(10),Background=Brushes.White,BorderBrush=new SolidColorBrush(Color.FromRgb(224,233,244)),BorderThickness=new Thickness(1),Cursor=Cursors.Hand};
    var stack=new StackPanel();
    var visual=new Border{Height=48,Background=new SolidColorBrush(Color.FromRgb(240,246,253)),CornerRadius=new CornerRadius(8),Margin=new Thickness(0,0,0,8),Child=new TextBlock{Text=IconFor(product.Name),FontSize=27,HorizontalAlignment=HorizontalAlignment.Center,VerticalAlignment=VerticalAlignment.Center}};
    stack.Children.Add(visual);stack.Children.Add(new TextBlock{Text=product.Name,FontWeight=FontWeights.SemiBold,FontSize=13,TextWrapping=TextWrapping.Wrap,MaxHeight=36,Foreground=new SolidColorBrush(Color.FromRgb(20,43,75))});
    stack.Children.Add(new TextBlock{Text=product.BarcodeText,FontSize=10,Foreground=new SolidColorBrush(Color.FromRgb(104,125,151)),Margin=new Thickness(0,3,0,3)});
    var unit=new TextBlock{Text=product.UnitText,FontSize=11,Foreground=new SolidColorBrush(Color.FromRgb(7,87,184)),Background=new SolidColorBrush(Color.FromRgb(229,239,255)),Padding=new Thickness(5,2,5,2),HorizontalAlignment=HorizontalAlignment.Left,Margin=new Thickness(0,0,0,4)};stack.Children.Add(unit);
    stack.Children.Add(new TextBlock{Text=product.Price.ToString("N2",Sq)+" lekë",FontSize=16,FontWeight=FontWeights.Bold,Foreground=new SolidColorBrush(Color.FromRgb(16,43,78))});card.Child=stack;
    card.MouseLeftButtonUp+=(s,e)=>AddProduct(product);card.ToolTip="Kliko për ta shtuar në faturë · Njësia: "+product.UnitText;
    card.MouseEnter+=(s,e)=>{card.BorderBrush=new SolidColorBrush(Color.FromRgb(8,102,214));card.Background=new SolidColorBrush(Color.FromRgb(250,252,255));};card.MouseLeave+=(s,e)=>{card.BorderBrush=new SolidColorBrush(Color.FromRgb(224,233,244));card.Background=Brushes.White;};ProductPanel.Children.Add(card);
   }
  }
  private string IconFor(string name){string n=(name??"").ToLowerInvariant();if(n.Contains("buk"))return "🥖";if(n.Contains("qumësht"))return "🥛";if(n.Contains("ujë"))return "💧";if(n.Contains("coca"))return "🥤";if(n.Contains("lays"))return "🍟";if(n.Contains("sheqer"))return "🍚";if(n.Contains("miell"))return "🌾";if(n.Contains("vaj"))return "🫒";if(n.Contains("oriz"))return "🍚";if(n.Contains("makaron"))return "🍝";if(n.Contains("kafe"))return "☕";if(n.Contains("biskot"))return "🍪";if(n.Contains("detergjent"))return "🧴";if(n.Contains("sapun"))return "🧼";if(n.Contains("letër"))return "🧻";return "▣";}

  private void AddProduct(Product p) {
   var existing=InvoiceLines.FirstOrDefault(x=>x.ProductId==p.Id);
   if(existing!=null)existing.Quantity+=1;
   else { var line=new InvoiceLine{ProductId=p.Id,Name=p.Name,Unit=p.UnitText,Price=p.Price,Quantity=1}; line.PropertyChanged+=(s,e)=>UpdateTotals(); InvoiceLines.Add(line); }
   UpdateTotals();
  }
  private void RenumberLines(){for(int i=0;i<InvoiceLines.Count;i++)InvoiceLines[i].Index=i+1;}
  private decimal Subtotal=>InvoiceLines.Sum(x=>x.LineTotal);
  private decimal DiscountPercent {get{decimal d;return decimal.TryParse(DiscountBox?.Text,out d)?Math.Max(0,Math.Min(100,d)):0;}}
  private decimal Total=>decimal.Round(Subtotal*(1-DiscountPercent/100m),2);
  private void UpdateTotals(){if(SubtotalText==null||TotalText==null)return;SubtotalText.Text=Subtotal.ToString("N2",Sq)+" lekë";TotalText.Text=Total.ToString("N2",Sq)+" lekë";}
  private void DiscountBox_TextChanged(object sender,TextChangedEventArgs e){UpdateTotals();}
  private void ClearInvoice_Click(object sender,RoutedEventArgs e){if(InvoiceLines.Count>0&&MessageBox.Show("Të pastrohet fatura aktuale?","ProShitje",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;InvoiceLines.Clear();UpdateTotals();}
  private void RemoveLine_Click(object sender,RoutedEventArgs e){var b=sender as Button;var line=b?.DataContext as InvoiceLine;if(line!=null)InvoiceLines.Remove(line);UpdateTotals();}
  private void InvoiceGrid_SelectionChanged(object sender,SelectionChangedEventArgs e){}
  private void Cash_Click(object sender,RoutedEventArgs e){paymentType="Cash";}
  private void Card_Click(object sender,RoutedEventArgs e){paymentType="Kartë";}
  private void OtherPayment_Click(object sender,RoutedEventArgs e){paymentType="Tjetër";}
  private void Scan_Click(object sender,RoutedEventArgs e){SearchBox.Focus();SearchBox.SelectAll();MessageBox.Show("Vendos fokusin te kërkimi dhe skano barkodin. Lexuesi i barkodit duhet të dërgojë Enter pas skanimit.","Skanim barkodi",MessageBoxButton.OK,MessageBoxImage.Information);}
  private void Window_KeyDown(object sender,KeyEventArgs e){if(e.Key==Key.F5){Fiscal_Click(this,new RoutedEventArgs());e.Handled=true;}else if(e.Key==Key.F6){SaveInvoice_Click(this,new RoutedEventArgs());e.Handled=true;}else if(e.Key==Key.F7){Print_Click(this,new RoutedEventArgs());e.Handled=true;}else if(e.Key==Key.Enter&&SearchBox.IsKeyboardFocusWithin){var q=SearchBox.Text.Trim();var p=currentProducts.FirstOrDefault(x=>string.Equals(x.Barcode,q,StringComparison.OrdinalIgnoreCase))??currentProducts.FirstOrDefault(x=>string.Equals(x.Code,q,StringComparison.OrdinalIgnoreCase));if(p!=null){AddProduct(p);SearchBox.SelectAll();e.Handled=true;}}}
  private void Fiscal_Click(object sender,RoutedEventArgs e){
   if(InvoiceLines.Count==0){MessageBox.Show("Fatura është bosh.","Skedar fiskal",MessageBoxButton.OK,MessageBoxImage.Warning);return;}
   try{string dir=AppConfig.FiscalOutputPath;if(string.IsNullOrWhiteSpace(dir))dir=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"Fiscal");Directory.CreateDirectory(dir);string file=Path.Combine(dir,"AED_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt");var sb=new StringBuilder();sb.AppendLine("CLEAR");sb.AppendLine("KEY REG");foreach(var line in InvoiceLines){string des=(line.Name??"").Replace("'","''").Replace("\r"," ").Replace("\n"," ");sb.AppendLine("SALE DPT=0,qty="+line.Quantity.ToString("0.###",CultureInfo.InvariantCulture)+",PRICE="+line.Price.ToString("0.00",CultureInfo.InvariantCulture)+",DES='"+des+"'");}sb.AppendLine("SUBTOT");sb.AppendLine("CLOSE TENDER=1");File.WriteAllText(file,sb.ToString(),new UTF8Encoding(false));MessageBox.Show("Skedari TXT u krijua:\n"+file+"\n\nKjo konfirmon vetëm krijimin e skedarit, jo pranimin nga pajisja fiskale.","Skedar fiskal",MessageBoxButton.OK,MessageBoxImage.Information);}catch(Exception ex){MessageBox.Show("Nuk u krijua skedari: "+ex.Message,"Gabim",MessageBoxButton.OK,MessageBoxImage.Error);}
  }
  private async void SaveInvoice_Click(object sender,RoutedEventArgs e){
   if(InvoiceLines.Count==0){MessageBox.Show("Fatura është bosh.","Ruaj faturën",MessageBoxButton.OK,MessageBoxImage.Warning);return;}
   if(string.IsNullOrWhiteSpace(AppConfig.AccessPath)||!File.Exists(AppConfig.AccessPath)){MessageBox.Show("Për ruajtjen reale duhet të zgjedhësh databazën MDB te Cilësimet. Në modalitetin demo nuk lejohet ruajtja e faturës.","MDB nuk është konfiguruar",MessageBoxButton.OK,MessageBoxImage.Warning);return;}
   if(MessageBox.Show("Do të ruhet fatura e shitjes në MDB dhe do të ulet stoku për çdo artikull. Të vazhdohet?","Konfirmo shitjen",MessageBoxButton.YesNo,MessageBoxImage.Question)!=MessageBoxResult.Yes)return;
   try{
    var db=new DatabaseService(AppConfig.AccessPath,AppConfig.AccessPassword);
    int id=await Task.Run(()=>db.SaveSale(CustomerBox.Text,paymentType,Total,DiscountPercent,AppConfig.CashierName,InvoiceLines.ToList()));
    MessageBox.Show("Fatura u ruajt në databazë. ID: "+id+"\nStoku u përditësua brenda të njëjtit transaksion.","Fatura u ruajt",MessageBoxButton.OK,MessageBoxImage.Information);
    InvoiceLines.Clear();DiscountBox.Text="0";invoiceNumber++;await LoadProductsAsync(SearchBox.Text);
   }catch(Exception ex){MessageBox.Show("Fatura nuk u ruajt. Ndryshimet e databazës u anuluan.\n\n"+ex.Message,"Gabim gjatë ruajtjes",MessageBoxButton.OK,MessageBoxImage.Error);}
  }
  private string Esc(string s)=>"\""+(s??"").Replace("\"","\"\"")+"\"";
  private void Print_Click(object sender,RoutedEventArgs e){if(InvoiceLines.Count==0){MessageBox.Show("Fatura është bosh.","Printo",MessageBoxButton.OK,MessageBoxImage.Warning);return;}var doc=new FlowDocument{FontFamily=new FontFamily("Segoe UI"),FontSize=12,PagePadding=new Thickness(36)};doc.Blocks.Add(new Paragraph(new Run("ProShitje — Fatura #"+invoiceNumber.ToString("D6"))){FontSize=18,FontWeight=FontWeights.Bold});doc.Blocks.Add(new Paragraph(new Run(DateTime.Now.ToString("dd.MM.yyyy HH:mm")+" · "+CustomerBox.Text)));var table=new Table();table.Columns.Add(new TableColumn{Width=new GridLength(220)});table.Columns.Add(new TableColumn{Width=new GridLength(55)});table.Columns.Add(new TableColumn{Width=new GridLength(55)});table.Columns.Add(new TableColumn{Width=new GridLength(70)});table.Columns.Add(new TableColumn{Width=new GridLength(75)});var group=new TableRowGroup();var header=new TableRow();foreach(string h in new[]{"Artikulli","Njësia","Sasia","Çmimi","Totali"})header.Cells.Add(new TableCell(new Paragraph(new Run(h)){FontWeight=FontWeights.Bold}));group.Rows.Add(header);foreach(var l in InvoiceLines){var row=new TableRow();foreach(string val in new[]{l.Name,l.Unit,l.Quantity.ToString(CultureInfo.InvariantCulture),l.Price.ToString("N2",Sq),l.LineTotal.ToString("N2",Sq)})row.Cells.Add(new TableCell(new Paragraph(new Run(val))));group.Rows.Add(row);}table.RowGroups.Add(group);doc.Blocks.Add(table);doc.Blocks.Add(new Paragraph(new Run("TOTAL: "+Total.ToString("N2",Sq)+" lekë")){FontSize=18,FontWeight=FontWeights.Bold,TextAlignment=TextAlignment.Right});var dlg=new PrintDialog();if(dlg.ShowDialog()==true)dlg.PrintDocument(((IDocumentPaginatorSource)doc).DocumentPaginator,"ProShitje Fature");}
  private void Settings_Click(object sender,RoutedEventArgs e){var win=new SettingsWindow{Owner=this};if(win.ShowDialog()==true){if(File.Exists(AppConfig.AccessPath))_=LoadProductsAsync(SearchBox.Text);else ShowDemoProducts(SearchBox.Text);}}
 }

 public sealed class SettingsWindow : Window {
  private readonly TextBox path=new TextBox(); private readonly PasswordBox password=new PasswordBox(); private readonly TextBox fiscal=new TextBox();
  public SettingsWindow(){Title="Cilësimet — ProShitje";Width=650;Height=340;WindowStartupLocation=WindowStartupLocation.CenterOwner;ResizeMode=ResizeMode.NoResize;Background=new SolidColorBrush(Color.FromRgb(243,247,252));
   var grid=new Grid{Margin=new Thickness(22)};for(int i=0;i<5;i++)grid.RowDefinitions.Add(new RowDefinition{Height=i==4?new GridLength(1,GridUnitType.Star):new GridLength(48)});grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(150)});grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)});
   AddLabel(grid,"Skedari MDB:",0);path.Text=AppConfig.AccessPath;Grid.SetRow(path,0);Grid.SetColumn(path,1);grid.Children.Add(path);var browse=new Button{Content="Zgjidh MDB…",Margin=new Thickness(0,5,0,5)};browse.Click+=(s,e)=>{var d=new OpenFileDialog{Filter="Access Database (*.mdb)|*.mdb",Title="Zgjidh databazën e ProShitje"};if(d.ShowDialog()==true)path.Text=d.FileName;};Grid.SetRow(browse,0);Grid.SetColumn(browse,1);browse.HorizontalAlignment=HorizontalAlignment.Right;grid.Children.Add(browse);
   AddLabel(grid,"Fjalëkalimi MDB:",1);Grid.SetRow(password,1);Grid.SetColumn(password,1);password.Password=AppConfig.AccessPassword;grid.Children.Add(password);
   AddLabel(grid,"Dosja fiskale TXT:",2);Grid.SetRow(fiscal,2);Grid.SetColumn(fiscal,1);fiscal.Text=AppConfig.FiscalOutputPath;grid.Children.Add(fiscal);
   var note=new TextBlock{Text="Lidhja me MDB përdor Jet OLEDB 4.0 (EXE x86). Ruajtja e faturës në tabelat reale do të aktivizohet pasi të verifikohet skema e plotë.",TextWrapping=TextWrapping.Wrap,Foreground=new SolidColorBrush(Color.FromRgb(85,105,130)),Margin=new Thickness(0,8,0,0)};Grid.SetRow(note,3);Grid.SetColumnSpan(note,2);grid.Children.Add(note);
   var buttons=new StackPanel{Orientation=Orientation.Horizontal,HorizontalAlignment=HorizontalAlignment.Right,VerticalAlignment=VerticalAlignment.Bottom};var test=new Button{Content="Testo MDB",Margin=new Thickness(0,0,8,0)};test.Click+=(s,e)=>{try{var db=new DatabaseService(path.Text,password.Password);db.TestConnection();MessageBox.Show("Lidhja me MDB funksionon.","Test MDB",MessageBoxButton.OK,MessageBoxImage.Information);}catch(Exception ex){MessageBox.Show(ex.Message,"Test MDB",MessageBoxButton.OK,MessageBoxImage.Error);}};var save=new Button{Content="Ruaj cilësimet",Background=new SolidColorBrush(Color.FromRgb(8,102,214)),Foreground=Brushes.White,Margin=new Thickness(0,0,8,0)};save.Click+=(s,e)=>Save();var cancel=new Button{Content="Mbyll"};cancel.Click+=(s,e)=>{DialogResult=false;};buttons.Children.Add(test);buttons.Children.Add(save);buttons.Children.Add(cancel);Grid.SetRow(buttons,4);Grid.SetColumnSpan(buttons,2);grid.Children.Add(buttons);Content=grid;
  }
  private void AddLabel(Grid g,string text,int row){var t=new TextBlock{Text=text,VerticalAlignment=VerticalAlignment.Center,Foreground=new SolidColorBrush(Color.FromRgb(37,58,84)),FontWeight=FontWeights.SemiBold};Grid.SetRow(t,row);Grid.SetColumn(t,0);g.Children.Add(t);}
  private void Save(){try{var config=ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);Set(config,"AccessPath",path.Text.Trim());Set(config,"AccessPassword",password.Password);Set(config,"FiscalOutputPath",fiscal.Text.Trim());config.Save(ConfigurationSaveMode.Modified);ConfigurationManager.RefreshSection("appSettings");DialogResult=true;}catch(Exception ex){MessageBox.Show("Nuk u ruajtën cilësimet. Nëse aplikacioni është instaluar në Program Files, provo ta ekzekutosh me leje të përshtatshme ose vendosi parametrat te ProShitjeWpf.exe.config.\n"+ex.Message,"Cilësimet",MessageBoxButton.OK,MessageBoxImage.Error);}}
  private void Set(Configuration config,string key,string value){if(config.AppSettings.Settings[key]==null)config.AppSettings.Settings.Add(key,value);else config.AppSettings.Settings[key].Value=value;}
 }
}

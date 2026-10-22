using Microsoft.Win32; using System; using System.Windows; using System.Windows.Controls;
namespace ProShitjeLicensing {
public partial class MainWindow : Window {
private DeviceNumbers device;
public MainWindow(){InitializeComponent(); DbPathBox.Text=LicenseService.FindDefaultDatabase(); RefreshDevice();}
private void RefreshButton_Click(object s,RoutedEventArgs e){RefreshDevice();}
private void RefreshDevice(){try{device=LicenseService.GetDeviceNumbers(); SsdSerialBox.Text=device.SsdSerial; N1Box.Text=device.N1.ToString(); N2Box.Text=device.N2.ToString(); StatusText.Text="Pajisja u identifikua. Dërgo N1/N2 administratorit.";}catch(Exception ex){StatusText.Text=ex.Message;}}
private void BrowseDb_Click(object s,RoutedEventArgs e){var d=new OpenFileDialog{Filter="Access database (*.mdb)|*.mdb"}; if(d.ShowDialog()==true)DbPathBox.Text=d.FileName;}
private void ActivateButton_Click(object s,RoutedEventArgs e){try{long key;if(device==null)RefreshDevice();if(!long.TryParse(LicenseKeyBox.Text.Trim(),out key))throw new Exception("Vendos serialin numerik.");if(!LicenseService.ValidateFormula(device.N1,device.N2,key))throw new Exception("Seriali nuk përputhet me N1/N2 dhe K=222.");if(string.IsNullOrWhiteSpace(DbPathBox.Text))throw new Exception("Zgjidh databazën MDB.");int months=Convert.ToInt32(((ComboBoxItem)DurationBox.SelectedItem).Tag);DateTime start=DateTime.Today, end=start.AddMonths(months);LicenseService.SaveToAccess(DbPathBox.Text,key.ToString(),start,end);StatusText.Text="Licenca u regjistrua deri më "+end.ToString("dd/MM/yyyy");MessageBox.Show(StatusText.Text,"ProShitje",MessageBoxButton.OK,MessageBoxImage.Information);}catch(Exception ex){MessageBox.Show(ex.Message,"Aktivizimi",MessageBoxButton.OK,MessageBoxImage.Warning);}}
}}
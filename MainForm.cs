using System;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ProShitje13 {
 public class MainForm : Form {
  readonly AppConfig cfg; readonly bool licensed; readonly DatabaseService db=new DatabaseService();
  readonly Color navy=Color.FromArgb(22,35,58), blue=Color.FromArgb(48,93,160), bg=Color.FromArgb(244,247,251), green=Color.FromArgb(15,145,105);
  Panel content=new Panel(); Label status=new Label(); TextBox searchBox; DataGridView productsGrid, basketGrid;
  DataTable basket; ComboBox customerCombo; Label totalLabel; Timer searchTimer; string currentPage="Dashboard"; bool searching=false;
  public MainForm(AppConfig c,bool lic) {
   cfg=c; licensed=lic; Text="ProShitje 1.3"; WindowState=FormWindowState.Maximized; MinimumSize=new Size(1120,720); Font=new Font("Segoe UI",10); BackColor=bg;
   basket=new DataTable(); basket.Columns.Add("Id",typeof(int)); basket.Columns.Add("Kodi",typeof(string)); basket.Columns.Add("Barkodi",typeof(string)); basket.Columns.Add("Artikulli",typeof(string)); basket.Columns.Add("Sasia",typeof(decimal)); basket.Columns.Add("Cmimi",typeof(decimal)); basket.Columns.Add("DPT",typeof(int)); basket.Columns.Add("Totali",typeof(decimal), "Sasia * Cmimi");
   searchTimer=new Timer{Interval=250}; searchTimer.Tick+=(s,e)=>{searchTimer.Stop(); _=LoadProductsAsync(searchBox==null?"":searchBox.Text);};
   BuildShell(); Shown+=(s,e)=>Connect();
  }
  void BuildShell(){
   var side=new Panel{Dock=DockStyle.Left,Width=225,BackColor=navy}; Controls.Add(side);
   side.Controls.Add(new Label{Text="PROSHITJE",ForeColor=Color.White,Font=new Font("Segoe UI",19,FontStyle.Bold),Left=22,Top=22,AutoSize=true});
   side.Controls.Add(new Label{Text="1.3  •  POS & MAGAZINË",ForeColor=Color.FromArgb(166,188,216),Left=24,Top=59,AutoSize=true});
   string[] menus={"Dashboard","Shitje","Artikujt","Hyrje magazine","Dalje magazine","Klientët","Furnitorët","Faturat","Raporte","Skadencat","Konfigurimet"};
   int y=103; foreach(string m in menus){var b=new Button{Text="   "+m,TextAlign=ContentAlignment.MiddleLeft,Left=10,Top=y,Width=205,Height=40,FlatStyle=FlatStyle.Flat,BackColor=navy,ForeColor=Color.FromArgb(230,237,247),Font=new Font("Segoe UI",10),Cursor=Cursors.Hand};b.FlatAppearance.BorderSize=0;b.FlatAppearance.MouseOverBackColor=Color.FromArgb(38,57,86);b.Click+=(s,e)=>Navigate(m);side.Controls.Add(b);y+=43;}
   var top=new Panel{Dock=DockStyle.Top,Height=76,BackColor=Color.White};Controls.Add(top);top.BringToFront();
   top.Controls.Add(new Label{Text="Sistemi i menaxhimit të shitjeve",Left=24,Top=10,AutoSize=true,Font=new Font("Segoe UI",16,FontStyle.Bold),ForeColor=Color.FromArgb(28,42,64)});
   top.Controls.Add(new Label{Text=(licensed?"Licencë aktive":"Mënyra e testimit")+"  •  "+cfg.DatabasePath,Left=26,Top=43,AutoSize=true,ForeColor=Color.Gray});
   content.Dock=DockStyle.Fill;content.Padding=new Padding(24);Controls.Add(content);content.BringToFront();
   status.Dock=DockStyle.Bottom;status.Height=28;status.BackColor=Color.White;status.Padding=new Padding(12,4,0,0);Controls.Add(status);status.BringToFront();
   Navigate("Dashboard");
  }
  void Navigate(string page){currentPage=page;searchTimer.Stop();content.Controls.Clear();searchBox=null;
   if(page=="Shitje"){BuildSales();return;} if(page=="Artikujt"){BuildProducts();return;}
   var title=Heading(page);content.Controls.Add(title);
   if(page=="Dashboard"){
    var flow=new FlowLayoutPanel{Dock=DockStyle.Top,Height=150,Padding=new Padding(0,15,0,0),WrapContents=false};content.Controls.Add(flow);
    foreach(var pair in new[]{new[]{"ARTIKUJT","Kërkim barkodi dhe emërtimi"},new[]{"SHITJE","Arkë dhe klientë"},new[]{"MAGAZINA","Hyrje / dalje"},new[]{"RAPORTE","Xhiro dhe analiza"}}){var card=new Panel{Width=235,Height=104,BackColor=Color.White,Margin=new Padding(0,0,16,0),Padding=new Padding(16)};card.Controls.Add(new Label{Text=pair[0],Font=new Font("Segoe UI",14,FontStyle.Bold),ForeColor=blue,Dock=DockStyle.Top,Height=30});card.Controls.Add(new Label{Text=pair[1],ForeColor=Color.Gray,Dock=DockStyle.Fill});flow.Controls.Add(card);}
    var info=new Label{Text="Gjendja e sistemit: "+(db.IsConnected?"Lidhur me databazën":"Duke kontrolluar lidhjen...")+"\r\n\r\nZgjidh Shitje për arkën ose Artikujt për kërkim të shpejtë. Skedari AED TXT nuk e konfirmon vetë fiskalizimin; kontrollo rezultatin në programin AED.",Dock=DockStyle.Top,Height=120,Padding=new Padding(4,20,0,0),ForeColor=Color.FromArgb(65,80,100)};content.Controls.Add(info);
   } else {
    content.Controls.Add(new Label{Text="Ky modul do të plotësohet pas verifikimit të fushave përkatëse në marketdb.mdb. Nuk kryhen shkrime të pasigurta në databazën ekzistuese.",Dock=DockStyle.Top,Height=100,ForeColor=Color.FromArgb(80,95,115)});
   }
  }
  Label Heading(string t)=>new Label{Text=t,Font=new Font("Segoe UI",23,FontStyle.Bold),ForeColor=Color.FromArgb(28,42,64),Dock=DockStyle.Top,Height=52};
  void Connect(){try{db.Connect(cfg.DatabasePath,cfg.DatabasePassword);status.Text="Databaza u lidh me sukses.";}catch(Exception ex){status.Text="Gabim databaze: "+ex.Message;}}
  DataGridView NewGrid(){return new DataGridView{Dock=DockStyle.Fill,ReadOnly=true,AllowUserToAddRows=false,MultiSelect=false,SelectionMode=DataGridViewSelectionMode.FullRowSelect,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill,BackgroundColor=Color.White,BorderStyle=BorderStyle.None,RowHeadersVisible=false,EnableHeadersVisualStyles=false,ColumnHeadersDefaultCellStyle={BackColor=Color.FromArgb(233,239,247),ForeColor=Color.FromArgb(35,50,72),Font=new Font("Segoe UI",9,FontStyle.Bold)},DefaultCellStyle={SelectionBackColor=Color.FromArgb(215,230,250),SelectionForeColor=Color.Black,Padding=new Padding(4)},RowTemplate={Height=31}};
  }
  TextBox SearchField(string placeholder,int width){var t=new TextBox{Width=width,Height=34,Font=new Font("Segoe UI",11),Text=placeholder,ForeColor=Color.Gray};t.GotFocus+=(s,e)=>{if(t.ForeColor==Color.Gray){t.Text="";t.ForeColor=Color.FromArgb(35,50,72);}};t.LostFocus+=(s,e)=>{if(String.IsNullOrWhiteSpace(t.Text)){t.Text=placeholder;t.ForeColor=Color.Gray;}};return t;}
  string SearchTerm(TextBox t)=>t==null||t.ForeColor==Color.Gray?"":t.Text.Trim();
  void BuildProducts(){
   content.Controls.Add(Heading("Artikujt"));
   var bar=new Panel{Dock=DockStyle.Top,Height=58};content.Controls.Add(bar);
   searchBox=SearchField("Kërko me barkod, kod ose emërtim...",560);searchBox.SetBounds(0,8,560,36);bar.Controls.Add(searchBox);
   var hint=new Label{Text="Shkruaj për kërkim automatik • skano barkodin",Left=575,Top=16,AutoSize=true,ForeColor=Color.Gray};bar.Controls.Add(hint);
   productsGrid=NewGrid();content.Controls.Add(productsGrid);productsGrid.BringToFront();bar.BringToFront();
   searchBox.TextChanged+=(s,e)=>{searchTimer.Stop();searchTimer.Start();};
   searchBox.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){searchTimer.Stop();_=LoadProductsAsync(SearchTerm(searchBox));e.SuppressKeyPress=true;}};
   productsGrid.DoubleClick+=(s,e)=>{if(currentPage=="Shitje")AddSelectedProduct();};
   _=LoadProductsAsync("");
  }
  async Task LoadProductsAsync(string term){
   if(productsGrid==null||productsGrid.IsDisposed||searching)return;
   searching=true;
   try{
    DataTable dt=await Task.Run(()=>DatabaseService.SearchProductsIndependent(cfg.DatabasePath,cfg.DatabasePassword,term));
    if(productsGrid!=null&&!productsGrid.IsDisposed){productsGrid.DataSource=dt;status.Text="Artikuj të gjetur: "+dt.Rows.Count;if(!String.IsNullOrWhiteSpace(term)&&dt.Rows.Count==1&&String.Equals(Convert.ToString(dt.Rows[0]["Barcodi"]),term,StringComparison.OrdinalIgnoreCase)&&currentPage=="Shitje")AddProductRow(dt.Rows[0]);}
   }catch(Exception ex){status.Text="Kërkimi dështoi: "+ex.Message;}
   finally{searching=false;}
  }
  void BuildSales(){
   content.Controls.Add(Heading("Shitje e shpejtë"));
   var tools=new Panel{Dock=DockStyle.Top,Height=102,BackColor=bg};content.Controls.Add(tools);
   searchBox=SearchField("Skano barkodin ose shkruaj kod / emërtim...",520);searchBox.SetBounds(0,8,520,38);tools.Controls.Add(searchBox);
   customerCombo=new ComboBox{Left=0,Top=55,Width=520,Height=32,DropDownStyle=ComboBoxStyle.DropDown};customerCombo.Items.Add("Klient pa emër / shitje e zakonshme");customerCombo.SelectedIndex=0;tools.Controls.Add(customerCombo);
   var customerHint=new Label{Text="Klienti (lista do të lidhet me tabelën Klienti)",Left=535,Top=62,AutoSize=true,ForeColor=Color.Gray};tools.Controls.Add(customerHint);
   var split=new SplitContainer{Dock=DockStyle.Fill,SplitterDistance=600,BackColor=bg,BorderStyle=BorderStyle.None};content.Controls.Add(split);split.BringToFront();tools.BringToFront();
   productsGrid=NewGrid();split.Panel1.Controls.Add(productsGrid);
   var resultHeader=new Label{Text="ARTIKUJT E GJETUR",Dock=DockStyle.Top,Height=32,ForeColor=blue,Font=new Font("Segoe UI",9,FontStyle.Bold)};split.Panel1.Controls.Add(resultHeader);resultHeader.BringToFront();
   basketGrid=NewGrid();basketGrid.ReadOnly=false;basketGrid.DataSource=basket;split.Panel2.Controls.Add(basketGrid);
   var basketHead=new Label{Text="SHPORTA E SHITJES",Dock=DockStyle.Top,Height=32,ForeColor=blue,Font=new Font("Segoe UI",9,FontStyle.Bold)};split.Panel2.Controls.Add(basketHead);basketHead.BringToFront();
   basketGrid.Columns["Id"].Visible=false;basketGrid.Columns["Kodi"].Visible=false;basketGrid.Columns["Barkodi"].Visible=false;basketGrid.Columns["DPT"].Visible=false;
   basketGrid.Columns["Sasia"].ReadOnly=false;basketGrid.Columns["Cmimi"].ReadOnly=false;basketGrid.Columns["Totali"].ReadOnly=true;
   var bottom=new Panel{Dock=DockStyle.Bottom,Height=126,BackColor=Color.White};split.Panel2.Controls.Add(bottom);bottom.BringToFront();
   totalLabel=new Label{Text="TOTAL  0.00",Left=12,Top=10,Width=360,Height=38,Font=new Font("Segoe UI",20,FontStyle.Bold),ForeColor=green};bottom.Controls.Add(totalLabel);
   var remove=Btn("Hiq artikull",Color.FromArgb(100,110,125));remove.SetBounds(12,57,120,34);remove.Click+=(s,e)=>{if(basketGrid.CurrentRow!=null&&basketGrid.CurrentRow.DataBoundItem is DataRowView v)basket.Rows.Remove(v.Row);UpdateTotal();};bottom.Controls.Add(remove);
   var export=Btn("Gjenero AED TXT",blue);export.SetBounds(142,57,165,34);export.Click+=(s,e)=>ExportAed();bottom.Controls.Add(export);
   var pay=Btn("Përfundo testin",green);pay.SetBounds(317,57,160,34);pay.Click+=(s,e)=>MessageBox.Show("Ky prototip ende nuk e regjistron shitjen në MDB dhe nuk konfirmon pagesën. Para përdorimit real do të implementojmë ruajtjen e faturës dhe detajeve.","Shitje në test");bottom.Controls.Add(pay);
   searchBox.TextChanged+=(s,e)=>{searchTimer.Stop();searchTimer.Start();};
   searchBox.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){string term=SearchTerm(searchBox);searchTimer.Stop();if(!String.IsNullOrWhiteSpace(term)){_=LoadProductsAsync(term);}e.SuppressKeyPress=true;}};
   productsGrid.DoubleClick+=(s,e)=>AddSelectedProduct();
   productsGrid.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){AddSelectedProduct();e.SuppressKeyPress=true;}};
   basket.ColumnChanged+=(s,e)=>UpdateTotal();basket.RowChanged+=(s,e)=>UpdateTotal();basket.RowDeleted+=(s,e)=>UpdateTotal();
   _=LoadProductsAsync("");
  }
  Button Btn(string t,Color c)=>new Button{Text=t,FlatStyle=FlatStyle.Flat,BackColor=c,ForeColor=Color.White,Font=new Font("Segoe UI",9,FontStyle.Bold),Cursor=Cursors.Hand};
  void AddSelectedProduct(){if(productsGrid?.CurrentRow==null)return;try{if(productsGrid.CurrentRow.DataBoundItem is DataRowView v)AddProductRow(v.Row);}catch{}}
  void AddProductRow(DataRow r){
   int id=ToInt(r["Id"]);foreach(DataRow old in basket.Rows){if(ToInt(old["Id"])==id){old["Sasia"]=ToDec(old["Sasia"])+1;UpdateTotal();return;}}
   DataRow row=basket.NewRow();row["Id"]=id;row["Kodi"]=Convert.ToString(r["Kodi"]);row["Barkodi"]=Convert.ToString(r["Barcodi"]);row["Artikulli"]=Convert.ToString(r["Artikulli"]);row["Sasia"]=1m;row["Cmimi"]=ToDec(r["Cmimi i daljes"]);row["DPT"]=0;basket.Rows.Add(row);UpdateTotal();
  }
  void UpdateTotal(){if(totalLabel==null||totalLabel.IsDisposed)return;decimal t=0;foreach(DataRow r in basket.Rows)t+=ToDec(r["Sasia"])*ToDec(r["Cmimi"]);totalLabel.Text="TOTAL  "+t.ToString("N2");}
  decimal ToDec(object v){if(v==null||v==DBNull.Value)return 0;decimal d;return Decimal.TryParse(Convert.ToString(v),NumberStyles.Any,CultureInfo.CurrentCulture,out d)?d:Convert.ToDecimal(v,CultureInfo.InvariantCulture);}
  int ToInt(object v){int n;return Int32.TryParse(Convert.ToString(v),out n)?n:0;}
  void ExportAed(){if(basket.Rows.Count==0){MessageBox.Show("Shporta është bosh.");return;}using(var sfd=new SaveFileDialog{Filter="AED TXT (*.txt)|*.txt",FileName="fatura.txt"})if(sfd.ShowDialog()==DialogResult.OK){var sb=new StringBuilder("CLEAR\r\nKEY REG\r\n\r\n");foreach(DataRow r in basket.Rows)sb.AppendLine(string.Format(CultureInfo.InvariantCulture,"SALE DPT={0},qty={1},PRICE={2},DES='{3}'",r["DPT"],ToDec(r["Sasia"]),ToDec(r["Cmimi"]),Convert.ToString(r["Artikulli"]).Replace("'","''")));sb.AppendLine("SUBTOT");sb.AppendLine("CLOSE TENDER=1");File.WriteAllText(sfd.FileName,sb.ToString(),new UTF8Encoding(false));MessageBox.Show("TXT u krijua. Verifikoje në AED. Shitja nuk është ruajtur ende në MDB.");}}
  protected override void OnFormClosed(FormClosedEventArgs e){searchTimer?.Stop();searchTimer?.Dispose();db.Dispose();base.OnFormClosed(e);}
 }
}

using System;
using System.Linq;
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
  Panel content=new Panel(); Label status=new Label(); TextBox searchBox; DataGridView productsGrid, basketGrid; FlowLayoutPanel productTiles; readonly System.Collections.Generic.Dictionary<string,Button> navButtons=new System.Collections.Generic.Dictionary<string,Button>(); Panel sidebar; Label pageTitle;
  DataTable basket; ComboBox customerCombo; Label totalLabel; Timer searchTimer; string currentPage="Dashboard"; bool searching=false; int searchRequestId=0; int selectedProductTile=-1; bool addFirstFilteredOnEnter=false;
  public MainForm(AppConfig c,bool lic) {
   cfg=c; licensed=lic; Text="ProShitje 1.3"; WindowState=FormWindowState.Maximized; MinimumSize=new Size(1000,650); Font=new Font("Segoe UI",10); BackColor=bg; KeyPreview=true;
   KeyDown+=(s,e)=>{ if(e.KeyCode==Keys.Enter && currentPage=="Shitje" && searchBox!=null && !searchBox.Focused){AddSelectedProductTile();e.SuppressKeyPress=true;} else if(e.KeyCode==Keys.F5){ e.SuppressKeyPress=true; GenerateFiscalTxt(); } else if(e.KeyCode==Keys.F6){e.SuppressKeyPress=true; MessageBox.Show("Ruajtja reale e faturës në MDB nuk është aktivizuar ende. Kjo shmang regjistrimin e një shitjeje të paplotë.", "F6 · Ruaj", MessageBoxButtons.OK, MessageBoxIcon.Warning);} else if(e.KeyCode==Keys.F7){e.SuppressKeyPress=true; MessageBox.Show("Printimi i faturës së shitjes nuk është lidhur ende. Eksporti AED TXT është funksion i veçantë.", "F7 · Printo", MessageBoxButtons.OK, MessageBoxIcon.Information);}};
   basket=new DataTable(); basket.Columns.Add("Id",typeof(int)); basket.Columns.Add("Kodi",typeof(string)); basket.Columns.Add("Barkodi",typeof(string)); basket.Columns.Add("Artikulli",typeof(string)); basket.Columns.Add("Njësia",typeof(string)); basket.Columns.Add("Sasia",typeof(decimal)); basket.Columns.Add("Cmimi",typeof(decimal)); basket.Columns.Add("DPT",typeof(int)); basket.Columns.Add("Totali",typeof(decimal), "Sasia * Cmimi");
   searchTimer=new Timer{Interval=250}; searchTimer.Tick+=(s,e)=>{searchTimer.Stop(); _=LoadProductsAsync(searchBox==null?"":searchBox.Text);};
   BuildShell(); Shown+=(s,e)=>Connect();
  }
  void BuildShell(){
   BackColor=Color.FromArgb(245,247,251); Font=new Font("Segoe UI",10f);
   sidebar=new Panel{Dock=DockStyle.Left,Width=238,BackColor=Color.FromArgb(20,32,53)}; Controls.Add(sidebar);
   var brand=new Panel{Dock=DockStyle.Top,Height=104,BackColor=Color.FromArgb(16,27,46)};sidebar.Controls.Add(brand);
   brand.Controls.Add(new Label{Text="P",TextAlign=ContentAlignment.MiddleCenter,Left=18,Top=23,Width=44,Height=44,BackColor=Color.FromArgb(43,105,210),ForeColor=Color.White,Font=new Font("Segoe UI",22,FontStyle.Bold)});
   brand.Controls.Add(new Label{Text="PROSHITJE",Left=72,Top=25,AutoSize=true,ForeColor=Color.White,Font=new Font("Segoe UI",16,FontStyle.Bold)});
   brand.Controls.Add(new Label{Text="MARKET  •  FARMACI",Left=73,Top=53,AutoSize=true,ForeColor=Color.FromArgb(155,174,201),Font=new Font("Segoe UI",8,FontStyle.Bold)});
   var nav=new Panel{Dock=DockStyle.Fill,AutoScroll=true,Padding=new Padding(10,14,10,10),BackColor=sidebar.BackColor};sidebar.Controls.Add(nav);nav.BringToFront();
   string[][] groups={new[]{"PËRMBLEDHJE","Dashboard"},new[]{"SHITJE","Shitje","Faturat","Klientët"},new[]{"MAGAZINA","Artikujt","Hyrje magazine","Dalje magazine","Skadencat"},new[]{"ADMINISTRIM","Furnitorët","Raporte","Konfigurimet"}};
   int y=0; foreach(var group in groups){var label=new Label{Text=group[0],Left=12,Top=y,Width=190,Height=24,ForeColor=Color.FromArgb(124,148,181),Font=new Font("Segoe UI",8,FontStyle.Bold)};nav.Controls.Add(label);y+=27;for(int i=1;i<group.Length;i++){string m=group[i];var b=new Button{Text="   "+NavGlyph(m)+"   "+m,TextAlign=ContentAlignment.MiddleLeft,Left=0,Top=y,Width=207,Height=42,FlatStyle=FlatStyle.Flat,BackColor=sidebar.BackColor,ForeColor=Color.FromArgb(218,227,240),Font=new Font("Segoe UI",9.5f),Cursor=Cursors.Hand,Tag=m};b.FlatAppearance.BorderSize=0;b.FlatAppearance.MouseOverBackColor=Color.FromArgb(34,51,77);b.Click+=(sender,e)=>Navigate(m);nav.Controls.Add(b);navButtons[m]=b;y+=44;}y+=12;}
   var sideFoot=new Panel{Dock=DockStyle.Bottom,Height=54,BackColor=Color.FromArgb(16,27,46)};sidebar.Controls.Add(sideFoot);sideFoot.Controls.Add(new Label{Text="●  SISTEM LOKAL",Left=17,Top=17,AutoSize=true,ForeColor=Color.FromArgb(104,211,169),Font=new Font("Segoe UI",9,FontStyle.Bold)});
   var top=new Panel{Dock=DockStyle.Top,Height=76,BackColor=Color.White};Controls.Add(top);top.BringToFront();
   pageTitle=new Label{Text="Dashboard",Left=26,Top=13,AutoSize=true,Font=new Font("Segoe UI",17,FontStyle.Bold),ForeColor=Color.FromArgb(28,42,64)};top.Controls.Add(pageTitle);
   top.Controls.Add(new Label{Text="Menaxhimi i shitjeve dhe magazinës",Left=28,Top=44,AutoSize=true,ForeColor=Color.FromArgb(120,132,150),Font=new Font("Segoe UI",9)});
   var dbPill=new Label{Text="●  MDB",AutoSize=true,Anchor=AnchorStyles.Top|AnchorStyles.Right,Top=23,ForeColor=Color.FromArgb(42,125,94),BackColor=Color.FromArgb(230,247,239),Padding=new Padding(12,7,12,7),Font=new Font("Segoe UI",9,FontStyle.Bold)};dbPill.Left=Width-160;top.Controls.Add(dbPill);top.Resize+=(s,e)=>dbPill.Left=top.ClientSize.Width-dbPill.Width-24;
   content.Dock=DockStyle.Fill;content.Padding=new Padding(26,22,26,24);content.BackColor=Color.FromArgb(245,247,251);Controls.Add(content);content.BringToFront();
   status.Dock=DockStyle.Bottom;status.Height=30;status.BackColor=Color.White;status.Padding=new Padding(14,6,0,0);status.ForeColor=Color.FromArgb(95,108,126);status.Font=new Font("Segoe UI",8.5f);Controls.Add(status);status.BringToFront();
   Navigate("Dashboard");
  }
  string NavGlyph(string m){switch(m){case "Dashboard":return "▦";case "Shitje":return "▣";case "Faturat":return "▤";case "Klientët":return "♙";case "Artikujt":return "▧";case "Hyrje magazine":return "↓";case "Dalje magazine":return "↑";case "Skadencat":return "◷";case "Furnitorët":return "♧";case "Raporte":return "▥";default:return "⚙";}}
  void Navigate(string page){currentPage=page;searchTimer.Stop();content.Controls.Clear();searchBox=null;
   if(pageTitle!=null)pageTitle.Text=page;
   foreach(var item in navButtons){bool active=item.Key==page;item.Value.BackColor=active?Color.FromArgb(43,91,160):sidebar.BackColor;item.Value.ForeColor=active?Color.White:Color.FromArgb(218,227,240);item.Value.FlatAppearance.MouseOverBackColor=active?Color.FromArgb(43,91,160):Color.FromArgb(34,51,77);}
   if(page=="Shitje"){BuildSales();return;} if(page=="Artikujt"){BuildProducts();return;}
   if(page=="Klientët"){BuildDataModule(page,"SELECT TOP 1000 [Id],[Emer Mbiemer],[Tel],[Cel],[email],[adrese],[VlereDebi],[Piketegrumbulluara] FROM [Klienti] ORDER BY [Emer Mbiemer]");return;}
   if(page=="Furnitorët"){BuildDataModule(page,"SELECT TOP 1000 [Id],[Emer Mbiemer],[Tel],[Cel],[email],[adrese],[Detyrim] FROM [Furnitori] ORDER BY [Emer Mbiemer]");return;}
   if(page=="Faturat"){BuildDataModule(page,"SELECT TOP 1000 [Id],[nrauto],[Data],[Ora],[Emri i kompanise],[Total],[Paguar],[Debitor],[Anulluar] FROM [Fature Hyrje-Dalje] ORDER BY [Data] DESC");return;}
   if(page=="Hyrje magazine"){BuildDataModule(page,"SELECT TOP 1000 [Id],[Kodi],[Barcodi],[Artikulli],[Sasihyrje],[Sasidalje],[Cmimi i hyrjes],[Cmimi i daljes],[data],[dateskadence],[Seria] FROM [Hyrje-Dalje] WHERE [Sasihyrje] > 0 ORDER BY [data] DESC");return;}
   if(page=="Dalje magazine"){BuildDataModule(page,"SELECT TOP 1000 [Id],[Kodi],[Barcodi],[Artikulli],[Sasihyrje],[Sasidalje],[Cmimi i hyrjes],[Cmimi i daljes],[data],[dateskadence],[Seria] FROM [Hyrje-Dalje] WHERE [Sasidalje] > 0 ORDER BY [data] DESC");return;}
   if(page=="Skadencat"){BuildDataModule(page,"SELECT TOP 1000 [Id],[Kodi],[Barcodi],[Artikulli],[dateskadence],[Seria],[Sasia Stock] FROM [Artikulli] WHERE [dateskadence] IS NOT NULL ORDER BY [dateskadence]");return;}
   if(page=="Konfigurimet"){BuildSettings();return;}
   var title=Heading(page);content.Controls.Add(title);
   if(page=="Dashboard"){
    var intro=new Panel{Dock=DockStyle.Top,Height=86,BackColor=Color.Transparent};content.Controls.Add(intro);
    intro.Controls.Add(new Label{Text="Mirë se erdhe në ProShitje",Left=0,Top=0,AutoSize=true,Font=new Font("Segoe UI",20,FontStyle.Bold),ForeColor=Color.FromArgb(27,42,65)});
    intro.Controls.Add(new Label{Text="Përmbledhje e punës dhe qasje e shpejtë në modulet kryesore.",Left=2,Top=39,AutoSize=true,Font=new Font("Segoe UI",10),ForeColor=Color.FromArgb(111,125,145)});
    var cards=new FlowLayoutPanel{Dock=DockStyle.Top,Height=142,WrapContents=false,AutoScroll=true,Padding=new Padding(0,5,0,8),BackColor=Color.Transparent};content.Controls.Add(cards);
    AddDashboardCard(cards,"ARTIKUJT","Katalogu i produkteve","▧",Color.FromArgb(53,111,207),"Artikujt");
    AddDashboardCard(cards,"SHITJE","Hap arkën e shitjes","▣",Color.FromArgb(19,151,111),"Shitje");
    AddDashboardCard(cards,"MAGAZINA","Hyrje dhe dalje","⇅",Color.FromArgb(220,143,45),"Hyrje magazine");
    AddDashboardCard(cards,"RAPORTE","Analiza dhe xhiro","▥",Color.FromArgb(130,91,190),"Raporte");
    var lower=new TableLayoutPanel{Dock=DockStyle.Fill,ColumnCount=2,RowCount=1,Padding=new Padding(0,10,0,0),BackColor=Color.Transparent};lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,58));lower.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,42));content.Controls.Add(lower);lower.BringToFront();
    var activity=WhiteCard("Puna e përditshme","Zgjidh një modul për të vazhduar.");activity.Dock=DockStyle.Fill;activity.Margin=new Padding(0,0,14,0);lower.Controls.Add(activity,0,0);
    var checklist=new Label{Text="✓  Kërkim i shpejtë sipas barkodit\r\n\r\n✓  Ndërfaqe e optimizuar për arkë\r\n\r\n✓  Lidhje me databazën Access\r\n\r\n!  Ruajtja e faturave dhe pagesave duhet verifikuar para përdorimit real.",Dock=DockStyle.Fill,Padding=new Padding(20,20,18,14),ForeColor=Color.FromArgb(75,91,113),Font=new Font("Segoe UI",10)};activity.Controls.Add(checklist);
    var connection=WhiteCard("Lidhja e databazës","Statusi i lidhjes");connection.Dock=DockStyle.Fill;connection.Margin=new Padding(0);lower.Controls.Add(connection,1,0);
    var state=new Label{Text=(db.IsConnected?"●  LIDHUR":"●  NUK ËSHTË LIDHUR")+"\r\n\r\n"+cfg.DatabasePath+"\r\n\r\n"+(licensed?"Licenca aktive":"Licencë testimi"),Dock=DockStyle.Fill,Padding=new Padding(20,20,18,14),ForeColor=db.IsConnected?Color.FromArgb(32,133,94):Color.FromArgb(190,115,37),Font=new Font("Segoe UI",10)};connection.Controls.Add(state);
   } else {
    content.Controls.Add(new Label{Text="Ky modul do të plotësohet pas verifikimit të fushave përkatëse në marketdb.mdb. Nuk kryhen shkrime të pasigurta në databazën ekzistuese.",Dock=DockStyle.Top,Height=100,ForeColor=Color.FromArgb(80,95,115)});
   }
  }
  Panel WhiteCard(string title,string subtitle){var p=new Panel{BackColor=Color.White,Padding=new Padding(0),Margin=new Padding(0)};p.Controls.Add(new Label{Text=title,Left=18,Top=16,AutoSize=true,Font=new Font("Segoe UI",12,FontStyle.Bold),ForeColor=Color.FromArgb(35,51,75)});p.Controls.Add(new Label{Text=subtitle,Left=20,Top=43,AutoSize=true,Font=new Font("Segoe UI",9),ForeColor=Color.FromArgb(126,139,157)});return p;}
  void AddDashboardCard(FlowLayoutPanel host,string title,string subtitle,string glyph,Color accent,string page){var p=new Panel{Width=230,Height=112,BackColor=Color.White,Margin=new Padding(0,0,14,0),Cursor=Cursors.Hand};var stripe=new Panel{Dock=DockStyle.Left,Width=4,BackColor=accent};p.Controls.Add(stripe);p.Controls.Add(new Label{Text=glyph,Left=17,Top=18,Width=42,Height=42,TextAlign=ContentAlignment.MiddleCenter,BackColor=Color.FromArgb(244,247,252),ForeColor=accent,Font=new Font("Segoe UI Symbol",20,FontStyle.Bold)});p.Controls.Add(new Label{Text=title,Left=70,Top=17,AutoSize=true,Font=new Font("Segoe UI",9,FontStyle.Bold),ForeColor=Color.FromArgb(104,119,139)});p.Controls.Add(new Label{Text=subtitle,Left=70,Top=39,AutoSize=true,Font=new Font("Segoe UI",10,FontStyle.Bold),ForeColor=Color.FromArgb(36,52,76)});p.Click+=(s,e)=>Navigate(page);foreach(Control c in p.Controls)c.Click+=(s,e)=>Navigate(page);host.Controls.Add(p);}
  Label Heading(string t)=>new Label{Text=t,Font=new Font("Segoe UI",22,FontStyle.Bold),ForeColor=Color.FromArgb(28,42,64),Dock=DockStyle.Top,Height=56};
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
   searchBox.KeyDown+=(s,e)=>{
    if(e.KeyCode==Keys.Enter){searchTimer.Stop();addFirstFilteredOnEnter=true;_=LoadProductsAsync(SearchTerm(searchBox));e.SuppressKeyPress=true;}
    else if(e.KeyCode==Keys.Down||e.KeyCode==Keys.Right||e.KeyCode==Keys.Up||e.KeyCode==Keys.Left){
     if(productTiles!=null&&productTiles.Controls.Count>0){int delta=(e.KeyCode==Keys.Down||e.KeyCode==Keys.Right)?1:-1;selectedProductTile=(selectedProductTile+delta+productTiles.Controls.Count)%productTiles.Controls.Count;HighlightProductTile();e.SuppressKeyPress=true;}
    }
   };
   productsGrid.DoubleClick+=(s,e)=>{if(currentPage=="Shitje")AddSelectedProduct();};
   _=LoadProductsAsync("");
  }
  async Task LoadProductsAsync(string term){
   if(productsGrid==null||productsGrid.IsDisposed)return;
   int request=++searchRequestId; var grid=productsGrid; string searched=(term??"").Trim();
   try{
    DataTable dt=await Task.Run(()=>DatabaseService.SearchProductsIndependent(cfg.DatabasePath,cfg.DatabasePassword,searched));
    if(request!=searchRequestId || grid.IsDisposed || grid!=productsGrid)return;
    grid.DataSource=dt;status.Text="Artikuj të gjetur: "+dt.Rows.Count;
    if(currentPage=="Shitje"){RenderProductTiles();selectedProductTile=productTiles!=null&&productTiles.Controls.Count>0?0:-1;HighlightProductTile();}
    if(addFirstFilteredOnEnter && currentPage=="Shitje" && searched.Length>0 && dt.Rows.Count>0){
      addFirstFilteredOnEnter=false;AddProductRow(dt.Rows[0]);status.Text="U shtua artikulli i parë nga kërkimi: "+Convert.ToString(dt.Rows[0]["Artikulli"]);if(searchBox!=null){searchBox.Clear();searchBox.Focus();}return;
    }
    addFirstFilteredOnEnter=false;
    if(currentPage=="Shitje"){RenderProductTiles();selectedProductTile=productTiles!=null&&productTiles.Controls.Count>0?0:-1;HighlightProductTile();}
    if(addFirstFilteredOnEnter && currentPage=="Shitje" && searched.Length>0 && dt.Rows.Count>0){
      addFirstFilteredOnEnter=false;AddProductRow(dt.Rows[0]);status.Text="U shtua artikulli i parë nga kërkimi: "+Convert.ToString(dt.Rows[0]["Artikulli"]);if(searchBox!=null){searchBox.Clear();searchBox.Focus();}return;
    }
    addFirstFilteredOnEnter=false;
    if(currentPage=="Shitje" && searched.Length>0 && dt.Rows.Count==1 &&
       (String.Equals(Convert.ToString(dt.Rows[0]["Barcodi"]).Trim(),searched,StringComparison.OrdinalIgnoreCase) || String.Equals(Convert.ToString(dt.Rows[0]["Kodi"]).Trim(),searched,StringComparison.OrdinalIgnoreCase))) {
      AddProductRow(dt.Rows[0]); if(searchBox!=null){searchBox.Clear();searchBox.Focus();}
    }
   }catch(Exception ex){if(request==searchRequestId)status.Text="Kërkimi dështoi: "+ex.Message;}
  }
  void BuildDataModule(string page,string sql){
   content.Controls.Add(Heading(page));
   var bar=new Panel{Dock=DockStyle.Top,Height=54,BackColor=bg};content.Controls.Add(bar);
   var filter=SearchField("Filtro listën e ngarkuar...",420);filter.SetBounds(0,8,420,34);bar.Controls.Add(filter);
   var note=new Label{Text="Pamje informative • të dhënat nuk ndryshohen nga kjo listë",Left=435,Top=17,AutoSize=true,ForeColor=Color.FromArgb(115,128,147)};bar.Controls.Add(note);
   var grid=NewGrid();content.Controls.Add(grid);grid.BringToFront();bar.BringToFront();
   try{var dt=DatabaseService.ReadTable(cfg.DatabasePath,cfg.DatabasePassword,sql);grid.DataSource=dt;filter.Text="";filter.ForeColor=Color.FromArgb(35,50,72);filter.TextChanged+=(s,e)=>{try{dt.DefaultView.RowFilter="";if(!String.IsNullOrWhiteSpace(filter.Text)){string term=filter.Text.Replace("'","''").Replace("[","[[]");var parts=new System.Collections.Generic.List<string>();foreach(DataColumn col in dt.Columns)if(col.DataType==typeof(string))parts.Add("Convert(["+col.ColumnName.Replace("]","]]" )+"], 'System.String') LIKE '%"+term+"%'");if(parts.Count>0)dt.DefaultView.RowFilter=String.Join(" OR ",parts.ToArray());} }catch{} };status.Text=page+": "+dt.Rows.Count+" rreshta (vetëm lexim).";}catch(Exception ex){status.Text="Nuk u hap moduli "+page+": "+ex.Message;grid.DataSource=new DataTable();}
  }
  void BuildSettings(){
   content.Controls.Add(Heading("Konfigurimet"));
   var card=WhiteCard("Lidhja me databazën Access","Ndryshimet ruhen në profilin lokal të Windows.");card.Dock=DockStyle.Top;card.Height=410;content.Controls.Add(card);card.BringToFront();
   var path=new TextBox{Left=22,Top=84,Width=600,Height=32,Text=cfg.DatabasePath};card.Controls.Add(path);
   var browse=Btn("Zgjidh MDB",blue);browse.SetBounds(635,82,130,34);browse.Click+=(s,e)=>{using(var d=new OpenFileDialog{Filter="Access 2003 (*.mdb)|*.mdb",FileName=path.Text})if(d.ShowDialog()==DialogResult.OK)path.Text=d.FileName;};card.Controls.Add(browse);
   var pwd=new TextBox{Left=22,Top=143,Width=300,Height=32,UseSystemPasswordChar=true,Text=cfg.DatabasePassword};card.Controls.Add(pwd);card.Controls.Add(new Label{Text="Fjalëkalimi i MDB",Left=22,Top=121,AutoSize=true,ForeColor=navy});
   var test=Btn("Testo lidhjen",blue);test.SetBounds(22,205,145,38);test.Click+=(s,e)=>{try{using(var d=new DatabaseService())d.Connect(path.Text.Trim(),pwd.Text);MessageBox.Show("Lidhja me MDB funksionon.","ProShitje");}catch(Exception ex){MessageBox.Show(ex.Message,"Gabim lidhjeje");}};card.Controls.Add(test);
   var save=Btn("Ruaj konfigurimin",green);save.SetBounds(178,205,175,38);save.Click+=(s,e)=>{try{cfg.DatabasePath=path.Text.Trim();cfg.DatabasePassword=pwd.Text;cfg.Save();db.Connect(cfg.DatabasePath,cfg.DatabasePassword);status.Text="Konfigurimi u ruajt dhe databaza u lidh.";MessageBox.Show("Konfigurimi u ruajt.","ProShitje");}catch(Exception ex){MessageBox.Show("Nuk u ruajt / lidh: "+ex.Message,"Gabim");}};card.Controls.Add(save);
   var fiscalLabel=new Label{Text="Dosja / path për skedarin TXT fiskal",Left=22,Top=260,AutoSize=true,ForeColor=navy};card.Controls.Add(fiscalLabel);
   var fiscalPath=new TextBox{Left=22,Top=284,Width=600,Height=32,Text=cfg.FiscalOutputPath};card.Controls.Add(fiscalPath);
   var fiscalBrowse=Btn("Zgjidh dosjen",blue);fiscalBrowse.SetBounds(635,282,130,34);fiscalBrowse.Click+=(s,e)=>{using(var d=new FolderBrowserDialog{SelectedPath=Directory.Exists(fiscalPath.Text)?fiscalPath.Text:AppDomain.CurrentDomain.BaseDirectory,Description="Zgjidh dosjen ku do të krijohet skedari fiskal TXT"})if(d.ShowDialog()==DialogResult.OK)fiscalPath.Text=d.SelectedPath;};card.Controls.Add(fiscalBrowse);
   var fiscalSave=Btn("Ruaj path fiskal",green);fiscalSave.SetBounds(22,330,170,38);fiscalSave.Click+=(s,e)=>{cfg.FiscalOutputPath=fiscalPath.Text.Trim();cfg.Save();MessageBox.Show("Path-i i skedarit fiskal u ruajt.","ProShitje");};card.Controls.Add(fiscalSave);
   var warning=new Label{Text="F5 krijon skedarin TXT në këtë dosje. Pajisja fiskale duhet të jetë e konfiguruar për ta lexuar atë format.",Left=22,Top=376,Width=800,Height=28,ForeColor=Color.FromArgb(170,95,35)};card.Controls.Add(warning);
  }
  void BuildSales(){
   content.Controls.Add(Heading("Arka e shitjes"));
   var tools=new Panel{Dock=DockStyle.Top,Height=154,BackColor=bg,Padding=new Padding(0,4,0,8)};
   var invoiceLabel=new Label{Text="NR. FATURËS: (e re)",Left=0,Top=0,Width=320,Height=28,Font=new Font("Segoe UI",12,FontStyle.Bold),ForeColor=navy};tools.Controls.Add(invoiceLabel);content.Controls.Add(tools);
   searchBox=SearchField("Skano barkodin ose kërko sipas emërtimit...",520);searchBox.SetBounds(0,31,Math.Max(360,content.ClientSize.Width-20),38);searchBox.Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right;tools.Controls.Add(searchBox);
   var scanHint=new Label{Text="ENTER: kërko / shto artikull     •     F5 Gjenero TXT fiskal     •     F6 Ruaj     •     F7 Printo",Left=2,Top=72,AutoSize=true,ForeColor=Color.FromArgb(90,105,125),Font=new Font("Segoe UI",9,FontStyle.Bold)};tools.Controls.Add(scanHint);
   customerCombo=new ComboBox{Left=0,Top=98,Width=520,Height=32,DropDownStyle=ComboBoxStyle.DropDownList,Font=new Font("Segoe UI",10)};
   try{var clients=DatabaseService.ReadTable(cfg.DatabasePath,cfg.DatabasePassword,"SELECT [Id],[Emer Mbiemer] FROM [Klienti] ORDER BY [Emer Mbiemer]");var blank=clients.NewRow();blank["Id"]=-1;blank["Emer Mbiemer"]="Klient pa emër / shitje e zakonshme";clients.Rows.InsertAt(blank,0);customerCombo.DataSource=clients;customerCombo.DisplayMember="Emer Mbiemer";customerCombo.ValueMember="Id";}catch{customerCombo.Items.Add("Klient pa emër / shitje e zakonshme");customerCombo.SelectedIndex=0;}
   tools.Controls.Add(customerCombo);
   var customerHint=new Label{Text="Klienti i faturës",Left=535,Top=105,AutoSize=true,ForeColor=Color.FromArgb(90,105,125)};tools.Controls.Add(customerHint);
   var salesLayout=new TableLayoutPanel{Dock=DockStyle.Fill,BackColor=bg,ColumnCount=2,RowCount=1,Margin=Padding.Empty,Padding=Padding.Empty};
   salesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,53f));salesLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,47f));salesLayout.RowStyles.Add(new RowStyle(SizeType.Percent,100f));
   var productsPanel=new Panel{Dock=DockStyle.Fill,BackColor=bg,Padding=new Padding(0,0,6,0)};
   var basketPanel=new Panel{Dock=DockStyle.Fill,BackColor=bg,Padding=new Padding(6,0,0,0)};
   salesLayout.Controls.Add(productsPanel,0,0);salesLayout.Controls.Add(basketPanel,1,0);content.Controls.Add(salesLayout);salesLayout.BringToFront();tools.BringToFront();
   productsGrid=NewGrid();productsGrid.Visible=false;productsGrid.DataBindingComplete+=(s,e)=>RenderProductTiles();productsPanel.Controls.Add(productsGrid);
   var resultHeader=new Label{Text="  PRODUKTET  ·  Prek kartën për ta shtuar",Dock=DockStyle.Top,Height=42,ForeColor=navy,Font=new Font("Segoe UI",10,FontStyle.Bold),TextAlign=ContentAlignment.MiddleLeft,BackColor=Color.FromArgb(235,241,249)};productsPanel.Controls.Add(resultHeader);resultHeader.BringToFront();
   productTiles=new FlowLayoutPanel{Dock=DockStyle.Fill,AutoScroll=true,WrapContents=true,BackColor=Color.White,Padding=new Padding(8),Margin=Padding.Empty};productsPanel.Controls.Add(productTiles);productTiles.BringToFront();resultHeader.BringToFront();productTiles.SizeChanged+=(s,e)=>RenderProductTiles();
   basketGrid=NewGrid();basketGrid.ReadOnly=false;basketGrid.DataSource=basket;basketGrid.Dock=DockStyle.Fill;basketPanel.Controls.Add(basketGrid);
   var basketHead=new Label{Text="  SHPORTA E SHITJES",Dock=DockStyle.Top,Height=38,ForeColor=blue,Font=new Font("Segoe UI",9,FontStyle.Bold),BackColor=Color.White,TextAlign=ContentAlignment.MiddleLeft};basketPanel.Controls.Add(basketHead);basketHead.BringToFront();
   basketGrid.Columns["Id"].Visible=false;basketGrid.Columns["Barkodi"].Visible=false;basketGrid.Columns["DPT"].Visible=false;
   basketGrid.Columns["Kodi"].Visible=true;basketGrid.Columns["Kodi"].HeaderText="Kodi";basketGrid.Columns["Kodi"].Width=82;
   basketGrid.Columns["Artikulli"].HeaderText="Emërtimi";
   basketGrid.Columns["Artikulli"].AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill;
   basketGrid.Columns["Njësia"].HeaderText="Njësia";basketGrid.Columns["Njësia"].Width=68;basketGrid.Columns["Njësia"].DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleCenter;
   basketGrid.Columns["Sasia"].HeaderText="Sasia";basketGrid.Columns["Sasia"].Width=76;basketGrid.Columns["Cmimi"].HeaderText="Çmimi";basketGrid.Columns["Cmimi"].Width=90;basketGrid.Columns["Totali"].HeaderText="Totali";basketGrid.Columns["Totali"].Width=100;
   basketGrid.Columns["Sasia"].ReadOnly=true;basketGrid.Columns["Cmimi"].ReadOnly=true;basketGrid.Columns["Totali"].ReadOnly=true;
   basketGrid.CellDoubleClick+=(s,e)=>{if(e.RowIndex>=0){var col=basketGrid.Columns[e.ColumnIndex].Name;if(col=="Sasia")EditBasketValue("Sasia","Ndrysho sasinë");else if(col=="Cmimi")EditBasketValue("Cmimi","Ndrysho çmimin");}};
   basketGrid.CellFormatting+=(s,e)=>{if(e.RowIndex>=0&&(basketGrid.Columns[e.ColumnIndex].Name=="Sasia"||basketGrid.Columns[e.ColumnIndex].Name=="Cmimi"||basketGrid.Columns[e.ColumnIndex].Name=="Totali"))e.CellStyle.Format="N2";};
   basketGrid.Columns["Artikulli"].DefaultCellStyle.WrapMode=DataGridViewTriState.True;
   basketGrid.RowTemplate.Height=52; productsGrid.RowTemplate.Height=52;
   var bottom=new Panel{Dock=DockStyle.Bottom,Height=202,BackColor=Color.White,Padding=new Padding(10)};basketPanel.Controls.Add(bottom);bottom.BringToFront();
   totalLabel=new Label{Text="TOTAL  0.00",Left=12,Top=8,Width=bottom.Width-24,Height=44,Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right,Font=new Font("Segoe UI",21,FontStyle.Bold),ForeColor=green,TextAlign=ContentAlignment.MiddleLeft};bottom.Controls.Add(totalLabel);
   int actionW=Math.Max(78,(bottom.ClientSize.Width-42)/4);
   var editQty=Btn("Sasia ±",Color.FromArgb(48,93,160));editQty.SetBounds(10,60,actionW,42);editQty.Anchor=AnchorStyles.Top|AnchorStyles.Left;editQty.Font=new Font("Segoe UI",9,FontStyle.Bold);editQty.Click+=(s,e)=>EditBasketValue("Sasia","Ndrysho sasinë");bottom.Controls.Add(editQty);
   var editPrice=Btn("Çmimi",Color.FromArgb(48,93,160));editPrice.SetBounds(editQty.Right+6,60,actionW,42);editPrice.Anchor=AnchorStyles.Top|AnchorStyles.Left;editPrice.Font=new Font("Segoe UI",9,FontStyle.Bold);editPrice.Click+=(s,e)=>EditBasketValue("Cmimi","Ndrysho çmimin");bottom.Controls.Add(editPrice);
   var remove=Btn("Hiq",Color.FromArgb(100,110,125));remove.SetBounds(editPrice.Right+6,60,actionW,42);remove.Anchor=AnchorStyles.Top|AnchorStyles.Left;remove.Font=new Font("Segoe UI",9,FontStyle.Bold);remove.Click+=(s,e)=>{if(basketGrid.CurrentRow!=null&&basketGrid.CurrentRow.DataBoundItem is DataRowView v)basket.Rows.Remove(v.Row);UpdateTotal();};bottom.Controls.Add(remove);
   var export=Btn("AED TXT",blue);export.SetBounds(remove.Right+6,60,actionW,42);export.Anchor=AnchorStyles.Top|AnchorStyles.Left;export.Font=new Font("Segoe UI",9,FontStyle.Bold);export.Click+=(s,e)=>ExportAed();bottom.Controls.Add(export);
   var touch=new TableLayoutPanel{Left=10,Top=112,Width=Math.Max(300,bottom.ClientSize.Width-20),Height=72,Anchor=AnchorStyles.Top|AnchorStyles.Left|AnchorStyles.Right,ColumnCount=3,RowCount=1};touch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.33f));touch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.33f));touch.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.34f));bottom.Controls.Add(touch);
   var f5=Btn("F5  FISKALE",Color.FromArgb(112,76,190));var f6=Btn("F6  RUAJ",Color.FromArgb(37,99,235));var f7=Btn("F7  PRINT",green);
   foreach(var b in new[]{f5,f6,f7}){b.Dock=DockStyle.Fill;b.Margin=new Padding(3);b.Font=new Font("Segoe UI",10,FontStyle.Bold);b.Height=64;}
   f5.Click+=(s,e)=>GenerateFiscalTxt();
   f6.Click+=(s,e)=>MessageBox.Show("Ruajtja reale e faturës në MDB nuk është aktivizuar ende. Nuk u regjistrua shitje.", "F6 · Ruaj",MessageBoxButtons.OK,MessageBoxIcon.Warning);
   f7.Click+=(s,e)=>MessageBox.Show("Printimi i faturës nuk është lidhur ende. Përdor AED TXT vetëm për testim të integrimit.", "F7 · Printo",MessageBoxButtons.OK,MessageBoxIcon.Information);
   touch.Controls.Add(f5,0,0);touch.Controls.Add(f6,1,0);touch.Controls.Add(f7,2,0);
   // Dock order: invoice header remains visible, actions stay at the bottom, grid fills the space between.
   basketHead.Dock=DockStyle.Top;basketHead.Height=40;
   bottom.Dock=DockStyle.Bottom;bottom.Height=198;
   basketGrid.Dock=DockStyle.Fill;
   basketPanel.Controls.SetChildIndex(basketHead,0);
   basketPanel.Controls.SetChildIndex(bottom,1);
   basketPanel.Controls.SetChildIndex(basketGrid,2);

   searchBox.TextChanged+=(s,e)=>{searchTimer.Stop();searchTimer.Start();};
   searchBox.KeyDown+=(s,e)=>{
    if(e.KeyCode==Keys.Enter){string term=SearchTerm(searchBox);searchTimer.Stop();addFirstFilteredOnEnter=!String.IsNullOrWhiteSpace(term);if(!String.IsNullOrWhiteSpace(term)){_=LoadProductsAsync(term);}else AddSelectedProductTile();e.SuppressKeyPress=true;}
    else if(e.KeyCode==Keys.Down||e.KeyCode==Keys.Right||e.KeyCode==Keys.Up||e.KeyCode==Keys.Left){if(productTiles!=null&&productTiles.Controls.Count>0){int delta=(e.KeyCode==Keys.Down||e.KeyCode==Keys.Right)?1:-1;selectedProductTile=(selectedProductTile+delta+productTiles.Controls.Count)%productTiles.Controls.Count;HighlightProductTile();e.SuppressKeyPress=true;}}
   };
   productsGrid.DoubleClick+=(s,e)=>{AddSelectedProduct();searchBox.Focus();searchBox.SelectAll();};
   productsGrid.KeyDown+=(s,e)=>{if(e.KeyCode==Keys.Enter){AddSelectedProduct();e.SuppressKeyPress=true;searchBox.Focus();searchBox.SelectAll();}};
   basket.ColumnChanged+=(s,e)=>UpdateTotal();basket.RowChanged+=(s,e)=>UpdateTotal();basket.RowDeleted+=(s,e)=>UpdateTotal();
   _=LoadProductsAsync("");
  }
  void RenderProductTiles(){
   if(productTiles==null||productsGrid==null||productTiles.IsDisposed)return;
   productTiles.SuspendLayout();productTiles.Controls.Clear();selectedProductTile=-1;
   int tileWidth=Math.Max(210,(productTiles.ClientSize.Width-34)/2);
   foreach(DataGridViewRow gr in productsGrid.Rows){
    if(gr.IsNewRow||gr.DataBoundItem==null)continue;
    var view=gr.DataBoundItem as DataRowView;if(view==null)continue;DataRow row=view.Row;
    string code=Convert.ToString(row.Table.Columns.Contains("Kodi")?row["Kodi"]:"");
    string unit=Convert.ToString(row.Table.Columns.Contains("Njësia e matjes")?row["Njësia e matjes"]:"");
    string name=Convert.ToString(row.Table.Columns.Contains("Artikulli")?row["Artikulli"]:"");
    decimal price=0;decimal.TryParse(Convert.ToString(row.Table.Columns.Contains("Cmimi i daljes")?row["Cmimi i daljes"]:"0"),out price);
    var tile=new Panel{Width=tileWidth,Height=142,BackColor=Color.FromArgb(248,250,253),Margin=new Padding(5),Padding=new Padding(10),BorderStyle=BorderStyle.FixedSingle,Cursor=Cursors.Hand};
    var codeLabel=new Label{Text="KODI: "+code+"   •   NJËSIA: "+unit,Left=8,Top=6,Width=tileWidth-24,Height=22,ForeColor=blue,Font=new Font("Segoe UI",8,FontStyle.Bold),AutoEllipsis=true};
    var nameLabel=new Label{Text=name,Left=8,Top=29,Width=tileWidth-24,Height=43,ForeColor=navy,Font=new Font("Segoe UI",10,FontStyle.Bold),AutoEllipsis=true};
    var priceLabel=new Label{Text=price.ToString("N2"),Left=8,Top=75,Width=tileWidth-75,Height=32,ForeColor=green,Font=new Font("Segoe UI",15,FontStyle.Bold)};
    var add=Btn("+",blue);add.SetBounds(tileWidth-58,78,44,42);add.Font=new Font("Segoe UI",16,FontStyle.Bold);add.TabStop=false;
    Action addAction=()=>{AddProductRow(row);status.Text="U shtua në faturë: "+name;searchBox?.Focus();};
    add.Click+=(sender,args)=>addAction();
    tile.Click+=(sender,args)=>{selectedProductTile=productTiles.Controls.IndexOf(tile);HighlightProductTile();addAction();};codeLabel.Click+=(sender,args)=>{selectedProductTile=productTiles.Controls.IndexOf(tile);HighlightProductTile();addAction();};nameLabel.Click+=(sender,args)=>{selectedProductTile=productTiles.Controls.IndexOf(tile);HighlightProductTile();addAction();};priceLabel.Click+=(sender,args)=>{selectedProductTile=productTiles.Controls.IndexOf(tile);HighlightProductTile();addAction();};
    tile.Controls.Add(codeLabel);tile.Controls.Add(nameLabel);tile.Controls.Add(priceLabel);tile.Controls.Add(add);productTiles.Controls.Add(tile);
   }
   productTiles.ResumeLayout();
  }
  void HighlightProductTile(){
   if(productTiles==null)return;
   for(int i=0;i<productTiles.Controls.Count;i++){
    var tile=productTiles.Controls[i];tile.BackColor=i==selectedProductTile?Color.FromArgb(218,233,253):Color.FromArgb(248,250,253);
    tile.Invalidate();
   }
  }
  void AddSelectedProductTile(){
   if(productTiles==null||productTiles.Controls.Count==0)return;
   if(selectedProductTile<0||selectedProductTile>=productTiles.Controls.Count)selectedProductTile=0;
   var tile=productTiles.Controls[selectedProductTile];
   var add=tile.Controls.OfType<Button>().FirstOrDefault();
   if(add!=null)add.PerformClick();
  }
  void ConfigureProductColumns(){
   if(productsGrid==null||productsGrid.Columns.Count==0)return;
   foreach(DataGridViewColumn c in productsGrid.Columns)c.Visible=true;
   if(productsGrid.Columns.Contains("Id"))productsGrid.Columns["Id"].Visible=false;
   if(productsGrid.Columns.Contains("Cmimihyrjes"))productsGrid.Columns["Cmimihyrjes"].Visible=false;
   if(productsGrid.Columns.Contains("Kodi")){productsGrid.Columns["Kodi"].HeaderText="KODI";productsGrid.Columns["Kodi"].Width=90;productsGrid.Columns["Kodi"].DisplayIndex=0;}
   if(productsGrid.Columns.Contains("Barcodi")){productsGrid.Columns["Barcodi"].HeaderText="BARKODI";productsGrid.Columns["Barcodi"].Width=120;productsGrid.Columns["Barcodi"].DisplayIndex=1;}
   if(productsGrid.Columns.Contains("Artikulli")){productsGrid.Columns["Artikulli"].HeaderText="EMËRTIMI";productsGrid.Columns["Artikulli"].AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill;productsGrid.Columns["Artikulli"].MinimumWidth=130;productsGrid.Columns["Artikulli"].DisplayIndex=2;productsGrid.Columns["Artikulli"].DefaultCellStyle.WrapMode=DataGridViewTriState.True;}
   if(productsGrid.Columns.Contains("Njësia e matjes")){productsGrid.Columns["Njësia e matjes"].HeaderText="NJËSIA";productsGrid.Columns["Njësia e matjes"].Width=75;productsGrid.Columns["Njësia e matjes"].DisplayIndex=3;}
   if(productsGrid.Columns.Contains("Cmimi i daljes")){productsGrid.Columns["Cmimi i daljes"].HeaderText="ÇMIMI";productsGrid.Columns["Cmimi i daljes"].Width=92;productsGrid.Columns["Cmimi i daljes"].DefaultCellStyle.Format="N2";productsGrid.Columns["Cmimi i daljes"].DefaultCellStyle.Alignment=DataGridViewContentAlignment.MiddleRight;productsGrid.Columns["Cmimi i daljes"].DisplayIndex=4;}
   if(productsGrid.Columns.Contains("Sasia Stock")){productsGrid.Columns["Sasia Stock"].HeaderText="GJENDJA";productsGrid.Columns["Sasia Stock"].Width=75;productsGrid.Columns["Sasia Stock"].DisplayIndex=5;}
   if(productsGrid.Columns.Contains("Cmimi"))productsGrid.Columns["Cmimi"].Visible=false;
   if(!productsGrid.Columns.Contains("AddButton")){var add=new DataGridViewButtonColumn{Name="AddButton",HeaderText="",Text="+ SHTO",UseColumnTextForButtonValue=true,Width=82,FlatStyle=FlatStyle.Flat};add.DefaultCellStyle.BackColor=Color.FromArgb(225,241,235);add.DefaultCellStyle.ForeColor=Color.FromArgb(17,120,82);add.DefaultCellStyle.Font=new Font("Segoe UI",9,FontStyle.Bold);productsGrid.Columns.Add(add);}
   productsGrid.Columns["AddButton"].DisplayIndex=productsGrid.Columns.Count-1;productsGrid.RowTemplate.Height=52;
  }
  void AddProductFromGridRow(int rowIndex){
   try {
    if(productsGrid==null || rowIndex<0 || rowIndex>=productsGrid.Rows.Count)return;
    var view=productsGrid.Rows[rowIndex].DataBoundItem as DataRowView;
    if(view==null)return;
    AddProductRow(view.Row);
    status.Text="U shtua në faturë: "+Convert.ToString(view.Row["Artikulli"]);
    if(searchBox!=null){searchBox.Focus();searchBox.SelectAll();}
   } catch(Exception ex) {
    status.Text="Nuk u shtua artikulli: "+ex.Message;
   }
  }
  void EditBasketValue(string column,string title){
   if(basketGrid.CurrentRow==null||!(basketGrid.CurrentRow.DataBoundItem is DataRowView view))return;
   decimal oldValue=Convert.ToDecimal(view.Row[column]);
   using(var f=new Form{Text=title,Width=390,Height=430,StartPosition=FormStartPosition.CenterParent,FormBorderStyle=FormBorderStyle.FixedDialog,MaximizeBox=false,MinimizeBox=false}){
    var label=new Label{Text=title,Left=16,Top=12,Width=340,Height=28,Font=new Font("Segoe UI",12,FontStyle.Bold)};
    var value=new TextBox{Left=16,Top=45,Width=340,Height=42,Font=new Font("Segoe UI",18,FontStyle.Bold),Text=oldValue.ToString(CultureInfo.CurrentCulture),TextAlign=HorizontalAlignment.Right};
    var keypad=new TableLayoutPanel{Left=16,Top=98,Width=340,Height=230,ColumnCount=3,RowCount=4};for(int i=0;i<3;i++)keypad.ColumnStyles.Add(new ColumnStyle(SizeType.Percent,33.33f));for(int i=0;i<4;i++)keypad.RowStyles.Add(new RowStyle(SizeType.Percent,25f));
    string[,] keys={{"7","8","9"},{"4","5","6"},{"1","2","3"},{"⌫","0","."}};
    for(int rr=0;rr<4;rr++)for(int cc=0;cc<3;cc++){string key=keys[rr,cc];var kb=Btn(key,Color.FromArgb(230,237,247));kb.ForeColor=navy;kb.Font=new Font("Segoe UI",15,FontStyle.Bold);kb.Dock=DockStyle.Fill;kb.Margin=new Padding(3);kb.Click+=(sender,args)=>{if(key=="⌫"){if(value.Text.Length>0)value.Text=value.Text.Substring(0,value.Text.Length-1);}else if(key=="."){if(!value.Text.Contains(".")&&!value.Text.Contains(","))value.Text+=CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;}else value.Text+=key;};keypad.Controls.Add(kb,cc,rr);}
    var ok=Btn("PRANO",green);ok.SetBounds(16,338,164,42);var cancel=Btn("ANULO",Color.FromArgb(100,110,125));cancel.SetBounds(192,338,164,42);
    ok.Click+=(s,e)=>{decimal parsed;if(!Decimal.TryParse(value.Text,NumberStyles.Number,CultureInfo.CurrentCulture,out parsed))Decimal.TryParse(value.Text,NumberStyles.Number,CultureInfo.InvariantCulture,out parsed);if(parsed<=0){MessageBox.Show("Vlera duhet të jetë më e madhe se zero.");return;}view.Row[column]=parsed;UpdateTotal();f.DialogResult=DialogResult.OK;f.Close();};
    cancel.Click+=(s,e)=>{f.DialogResult=DialogResult.Cancel;f.Close();};f.Controls.Add(label);f.Controls.Add(value);f.Controls.Add(keypad);f.Controls.Add(ok);f.Controls.Add(cancel);f.AcceptButton=ok;f.CancelButton=cancel;f.Shown+=(s,e)=>{value.SelectAll();value.Focus();};f.ShowDialog(this);
   }
  }
  void GenerateFiscalTxt(){
   try{
    if(basket==null||basket.Rows.Count==0){MessageBox.Show("Shporta është bosh.","F5 · TXT fiskal",MessageBoxButtons.OK,MessageBoxIcon.Warning);return;}
    string folder=(cfg.FiscalOutputPath??"").Trim();
    if(String.IsNullOrWhiteSpace(folder)){MessageBox.Show("Vendos fillimisht path-in te Konfigurimet → Dosja / path për skedarin TXT fiskal.","F5 · TXT fiskal",MessageBoxButtons.OK,MessageBoxIcon.Warning);Navigate("Konfigurimet");return;}
    Directory.CreateDirectory(folder);
    var sb=new StringBuilder();sb.AppendLine("CLEAR");sb.AppendLine("KEY REG");sb.AppendLine();
    foreach(DataRow row in basket.Rows){
     string desc=Convert.ToString(row["Artikulli"]).Replace("'","''");
     decimal qty=Convert.ToDecimal(row["Sasia"]);decimal price=Convert.ToDecimal(row["Cmimi"]);int dpt=Convert.ToInt32(row["DPT"]);
     sb.AppendLine("SALE DPT="+dpt.ToString(CultureInfo.InvariantCulture)+",qty="+qty.ToString("0.####",CultureInfo.InvariantCulture)+",PRICE="+price.ToString("0.00",CultureInfo.InvariantCulture)+",DES='"+desc+"'");
    }
    sb.AppendLine("SUBTOT");sb.AppendLine("CLOSE TENDER=1");
    string file=Path.Combine(folder,"ProShitje_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".txt");
    File.WriteAllText(file,sb.ToString(),new UTF8Encoding(false));
    MessageBox.Show("Skedari TXT u krijua:\r\n"+file+"\r\n\r\nKjo konfirmon vetëm krijimin e skedarit; pranimi nga pajisja fiskale duhet verifikuar veçmas.","F5 · TXT fiskal",MessageBoxButtons.OK,MessageBoxIcon.Information);
   }catch(Exception ex){MessageBox.Show("Nuk u krijua skedari TXT:\r\n"+ex.Message,"Gabim fiskal",MessageBoxButtons.OK,MessageBoxIcon.Error);}
  }
  Button Btn(string t,Color c)=>new Button{Text=t,FlatStyle=FlatStyle.Flat,BackColor=c,ForeColor=Color.White,Font=new Font("Segoe UI",9,FontStyle.Bold),Cursor=Cursors.Hand};
  void AddSelectedProduct(){if(productsGrid?.CurrentRow==null)return;try{if(productsGrid.CurrentRow.DataBoundItem is DataRowView v)AddProductRow(v.Row);}catch{}}
  void AddProductRow(DataRow r){
   int id=ToInt(r["Id"]);foreach(DataRow old in basket.Rows){if(ToInt(old["Id"])==id){old["Sasia"]=ToDec(old["Sasia"])+1;UpdateTotal();return;}}
   DataRow row=basket.NewRow();row["Id"]=id;row["Kodi"]=Convert.ToString(r["Kodi"]);row["Barkodi"]=Convert.ToString(r["Barcodi"]);row["Artikulli"]=Convert.ToString(r["Artikulli"]);row["Njësia"]=r.Table.Columns.Contains("Njësia e matjes")?Convert.ToString(r["Njësia e matjes"]):"";row["Sasia"]=1m;row["Cmimi"]=ToDec(r["Cmimi i daljes"]);row["DPT"]=0;basket.Rows.Add(row);UpdateTotal();
  }
  void UpdateTotal(){if(totalLabel==null||totalLabel.IsDisposed)return;decimal t=0;foreach(DataRow r in basket.Rows)t+=ToDec(r["Sasia"])*ToDec(r["Cmimi"]);totalLabel.Text="TOTAL  "+t.ToString("N2");}
  decimal ToDec(object v){if(v==null||v==DBNull.Value)return 0;decimal d;return Decimal.TryParse(Convert.ToString(v),NumberStyles.Any,CultureInfo.CurrentCulture,out d)?d:Convert.ToDecimal(v,CultureInfo.InvariantCulture);}
  int ToInt(object v){int n;return Int32.TryParse(Convert.ToString(v),out n)?n:0;}
  void ExportAed(){if(basket.Rows.Count==0){MessageBox.Show("Shporta është bosh.");return;}using(var sfd=new SaveFileDialog{Filter="AED TXT (*.txt)|*.txt",FileName="fatura.txt"})if(sfd.ShowDialog()==DialogResult.OK){var sb=new StringBuilder("CLEAR\r\nKEY REG\r\n\r\n");foreach(DataRow r in basket.Rows)sb.AppendLine(string.Format(CultureInfo.InvariantCulture,"SALE DPT={0},qty={1},PRICE={2},DES='{3}'",r["DPT"],ToDec(r["Sasia"]),ToDec(r["Cmimi"]),Convert.ToString(r["Artikulli"]).Replace("'","''")));sb.AppendLine("SUBTOT");sb.AppendLine("CLOSE TENDER=1");File.WriteAllText(sfd.FileName,sb.ToString(),new UTF8Encoding(false));MessageBox.Show("TXT u krijua. Verifikoje në AED. Shitja nuk është ruajtur ende në MDB.");}}
  protected override void OnFormClosed(FormClosedEventArgs e){searchTimer?.Stop();searchTimer?.Dispose();db.Dispose();base.OnFormClosed(e);}
 }
}

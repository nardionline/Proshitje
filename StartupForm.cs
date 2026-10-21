using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
namespace ProShitje13 {
 public class StartupForm : Form {
  AppConfig cfg=AppConfig.Load();
  TextBox db=new TextBox(), pwd=new TextBox(), serial=new TextBox();
  Label status=new Label();
  public StartupForm() {
   Text="ProShitje 1.3 | Konfigurimi"; StartPosition=FormStartPosition.CenterScreen; Size=new Size(760,560); MinimumSize=new Size(700,520);
   BackColor=Color.FromArgb(244,247,251); Font=new Font("Segoe UI",10);
   var header=new Panel{Dock=DockStyle.Top,Height=100,BackColor=Color.FromArgb(22,35,58)};
   var title=new Label{Text="ProShitje 1.3",ForeColor=Color.White,Font=new Font("Segoe UI",24,FontStyle.Bold),Left=28,Top=18,AutoSize=true};
   var sub=new Label{Text="Konfigurimi i sistemit dhe aktivizimi",ForeColor=Color.FromArgb(194,208,228),Left=31,Top=61,AutoSize=true};
   header.Controls.Add(title);header.Controls.Add(sub);Controls.Add(header);
   var panel=new Panel{Dock=DockStyle.Fill,Padding=new Padding(28)}; Controls.Add(panel);panel.BringToFront();
   AddLabel(panel,"Skedari i databazës (.mdb)",28,22);
   db.SetBounds(28,52,520,34); db.Text=cfg.DatabasePath; panel.Controls.Add(db);
   var browse=Btn("Zgjidh...",Color.FromArgb(55,93,160));browse.SetBounds(560,51,120,36);browse.Click+=(s,e)=>{using(var d=new OpenFileDialog{Filter="Access 2003 (*.mdb)|*.mdb",FileName=db.Text})if(d.ShowDialog()==DialogResult.OK)db.Text=d.FileName;};panel.Controls.Add(browse);
   AddLabel(panel,"Fjalëkalimi i databazës",28,103);
   pwd.SetBounds(28,133,300,34);pwd.UseSystemPasswordChar=true;pwd.Text=cfg.DatabasePassword;panel.Controls.Add(pwd);
   AddLabel(panel,"Seriali i licencës",360,103);
   serial.SetBounds(360,133,320,34);serial.Text=cfg.LicenseSerial;panel.Controls.Add(serial);
   var note=new Label{Text="Konfigurimi ruhet te AppData\\Local\\ProShitje13\\config.xml. Mbaj një kopje rezervë të MDB-së para përdorimit.",Left=28,Top=190,Width=650,Height=55,ForeColor=Color.FromArgb(85,99,117)};panel.Controls.Add(note);
   var test=Btn("Testo databazën",Color.FromArgb(55,93,160));test.SetBounds(28,260,190,42);test.Click+=(s,e)=>TestDb();panel.Controls.Add(test);
   var activate=Btn("Aktivizo dhe vazhdo",Color.FromArgb(15,145,105));activate.SetBounds(230,260,220,42);activate.Click+=(s,e)=>Activate();panel.Controls.Add(activate);
   var open=Btn("Hap pa serial (test)",Color.FromArgb(100,110,125));open.SetBounds(462,260,218,42);open.Click+=(s,e)=>{cfg.DatabasePath=db.Text.Trim();cfg.DatabasePassword=pwd.Text;cfg.LicenseSerial=serial.Text.Trim();cfg.Save(); OpenMain(false);};panel.Controls.Add(open);
   status.SetBounds(28,330,650,70);status.ForeColor=Color.FromArgb(55,75,95);panel.Controls.Add(status);
  }
  void AddLabel(Control p,string t,int x,int y){p.Controls.Add(new Label{Text=t,Left=x,Top=y,AutoSize=true,ForeColor=Color.FromArgb(40,53,73),Font=new Font("Segoe UI",9,FontStyle.Bold)});}
  Button Btn(string t,Color c)=>new Button{Text=t,FlatStyle=FlatStyle.Flat,BackColor=c,ForeColor=Color.White,Font=new Font("Segoe UI",9,FontStyle.Bold),Cursor=Cursors.Hand};
  void TestDb(){try{using(var dbs=new DatabaseService())dbs.Connect(db.Text.Trim(),pwd.Text);status.Text="Lidhja u testua me sukses. U gjet tabela Artikulli.";status.ForeColor=Color.SeaGreen;}catch(Exception ex){status.Text="Lidhja dështoi: "+ex.Message;status.ForeColor=Color.Firebrick;}}
  void Activate(){if(!LicenseService.Validate(serial.Text)){MessageBox.Show("Seriali nuk është i vlefshëm. Aktivizimi real kërkon serial të gjeneruar nga botuesi.","Aktivizimi");return;}cfg.LicenseSerial=serial.Text.Trim();cfg.DatabasePath=db.Text.Trim();cfg.DatabasePassword=pwd.Text;cfg.Save();OpenMain(true);}
  void OpenMain(bool licensed){cfg.DatabasePath=db.Text.Trim();cfg.DatabasePassword=pwd.Text;cfg.LicenseSerial=serial.Text.Trim();cfg.Save();var f=new MainForm(cfg,licensed);Hide();f.FormClosed+=(s,e)=>Close();f.Show();}
 }
}
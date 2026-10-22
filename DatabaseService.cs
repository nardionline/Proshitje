using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Threading.Tasks;
namespace ProShitje.Wpf {
 public sealed class DatabaseService {
  private readonly string path; private readonly string password;
  public DatabaseService(string mdbPath,string mdbPassword=""){path=mdbPath??"";password=mdbPassword??"";}
  private OleDbConnection Open(){if(string.IsNullOrWhiteSpace(path)||!File.Exists(path))throw new FileNotFoundException("Zgjidh skedarin MDB te Cilësimet.",path);string cs="Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\""+path.Replace("\"","")+"\";";if(password.Length>0)cs+="Jet OLEDB:Database Password=\""+password.Replace("\"","")+"\";";var c=new OleDbConnection(cs);c.Open();return c;}
  public Task<List<Product>> SearchAsync(string term)=>Task.Run(()=>Search(term));
  private List<Product> Search(string term){var list=new List<Product>();using(var c=Open())using(var cmd=c.CreateCommand()){
   string sql="SELECT TOP 250 [Id],[Kodi],[Barcodi],[Artikulli],[Njësia e matjes],[Cmimi i daljes],[Sasia Stock] FROM [Artikulli]";
   if(!string.IsNullOrWhiteSpace(term)){sql+=" WHERE [Artikulli] LIKE ? OR [Barcodi] LIKE ? OR [Kodi] LIKE ?";string q="%"+term.Trim()+"%";cmd.Parameters.Add("p1",OleDbType.VarWChar,255).Value=q;cmd.Parameters.Add("p2",OleDbType.VarWChar,255).Value=q;cmd.Parameters.Add("p3",OleDbType.VarWChar,255).Value=q;}
   sql+=" ORDER BY [Artikulli]";cmd.CommandText=sql;using(var r=cmd.ExecuteReader()){while(r!=null&&r.Read()){list.Add(new Product{Id=Convert.ToInt32(r["Id"]),Code=Safe(r["Kodi"]),Barcode=Safe(r["Barcodi"]),Name=Safe(r["Artikulli"]),Unit=Safe(r["Njësia e matjes"]),Price=Money(r["Cmimi i daljes"]),Stock=Money(r["Sasia Stock"]) });}}
  }return list;}
  private static string Safe(object v)=>v==null||v==DBNull.Value?"":Convert.ToString(v);
  private static decimal Money(object v){decimal d;return v==null||v==DBNull.Value?0:decimal.TryParse(Convert.ToString(v),out d)?d:0;}

  public int SaveSale(string customer, string paymentType, decimal total, decimal discountPercent, string user, IList<InvoiceLine> lines) {
   if(lines==null||lines.Count==0) throw new InvalidOperationException("Fatura nuk ka artikuj.");
   using(var c=Open()) using(var tx=c.BeginTransaction()) {
    try {
     using(var cmd=c.CreateCommand()) {
      cmd.Transaction=tx;
      cmd.CommandText="INSERT INTO [Fature Hyrje-Dalje] ([Data],[Ora],[Shitur],[faturadalese],[faturahyrese],[Total],[Totalmetvsh],[Paguar],[VlerePaguar],[Kasa],[Perdoruesi],[SkontoK],[Anulluar],[Debitor]) VALUES (?,?,?,?,?,?,?,?,?,?,?,?,?,?)";
      cmd.Parameters.Add("p1",OleDbType.Date).Value=DateTime.Today;
      cmd.Parameters.Add("p2",OleDbType.Date).Value=DateTime.Now;
      cmd.Parameters.Add("p3",OleDbType.VarWChar,50).Value=string.IsNullOrWhiteSpace(customer)?"Klient pa emer":customer.Trim();
      cmd.Parameters.Add("p4",OleDbType.Integer).Value=1;
      cmd.Parameters.Add("p5",OleDbType.Integer).Value=0;
      cmd.Parameters.Add("p6",OleDbType.Single).Value=(float)total;
      cmd.Parameters.Add("p7",OleDbType.Single).Value=(float)total;
      cmd.Parameters.Add("p8",OleDbType.Single).Value=(float)total;
      cmd.Parameters.Add("p9",OleDbType.Single).Value=(float)total;
      cmd.Parameters.Add("p10",OleDbType.VarWChar,50).Value=paymentType??"Cash";
      cmd.Parameters.Add("p11",OleDbType.VarWChar,50).Value=string.IsNullOrWhiteSpace(user)?Environment.UserName:user;
      cmd.Parameters.Add("p12",OleDbType.Single).Value=(float)discountPercent;
      cmd.Parameters.Add("p13",OleDbType.VarWChar,50).Value="Jo";
      cmd.Parameters.Add("p14",OleDbType.VarWChar,50).Value="Jo";
      cmd.ExecuteNonQuery();
     }
     int invoiceId;
     using(var cmd=c.CreateCommand()){cmd.Transaction=tx;cmd.CommandText="SELECT @@IDENTITY";invoiceId=Convert.ToInt32(cmd.ExecuteScalar());}
     foreach(var line in lines) {
      if(line.Quantity<=0) throw new InvalidOperationException("Sasia duhet të jetë pozitive: "+line.Name);
      using(var check=c.CreateCommand()) {check.Transaction=tx;check.CommandText="SELECT [Sasia Stock] FROM [Artikulli] WHERE [Id]=?";check.Parameters.Add("id",OleDbType.Integer).Value=line.ProductId;object v=check.ExecuteScalar();if(v==null||v==DBNull.Value)throw new InvalidOperationException("Artikulli nuk u gjet: "+line.Name);decimal stock=Money(v);if(stock<line.Quantity)throw new InvalidOperationException("Stok i pamjaftueshëm për '"+line.Name+"'. Gjendja: "+stock);}
      using(var d=c.CreateCommand()) {d.Transaction=tx;d.CommandText="INSERT INTO [Hyrje-Dalje] ([Kodi],[Barcodi],[Artikulli],[Njësia e matjes],[Cmimi i daljes],[Sasihyrje],[Sasidalje],[hyrje-dalje],[idart],[data],[Totali],[Totalmetvsh],[Totalpatvsh]) SELECT [Kodi],[Barcodi],[Artikulli],[Njësia e matjes],?,?,?,?,?,?,?, ?,? FROM [Artikulli] WHERE [Id]=?";
       d.Parameters.Add("p1",OleDbType.Single).Value=(float)line.Price;d.Parameters.Add("p2",OleDbType.Single).Value=0f;d.Parameters.Add("p3",OleDbType.Single).Value=(float)line.Quantity;d.Parameters.Add("p4",OleDbType.Integer).Value=invoiceId;d.Parameters.Add("p5",OleDbType.Integer).Value=line.ProductId;d.Parameters.Add("p6",OleDbType.Date).Value=DateTime.Now;d.Parameters.Add("p7",OleDbType.Single).Value=(float)line.LineTotal;d.Parameters.Add("p8",OleDbType.Single).Value=(float)line.LineTotal;d.Parameters.Add("p9",OleDbType.Single).Value=(float)line.LineTotal;d.Parameters.Add("p10",OleDbType.Integer).Value=line.ProductId; if(d.ExecuteNonQuery()!=1)throw new InvalidOperationException("Nuk u ruajt rreshti: "+line.Name);
      }
      using(var u=c.CreateCommand()){u.Transaction=tx;u.CommandText="UPDATE [Artikulli] SET [Sasia Stock]=Nz([Sasia Stock],0)-? WHERE [Id]=?";u.Parameters.Add("q",OleDbType.Single).Value=(float)line.Quantity;u.Parameters.Add("id",OleDbType.Integer).Value=line.ProductId;u.ExecuteNonQuery();}
     }
     tx.Commit();return invoiceId;
    } catch {try{tx.Rollback();}catch{} throw;}
   }
  }
  public bool TestConnection(){using(var c=Open())using(var cmd=new OleDbCommand("SELECT TOP 1 [Id] FROM [Artikulli]",c)){cmd.ExecuteScalar();return true;}}
 }
}

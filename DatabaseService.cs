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
  public bool TestConnection(){using(var c=Open())using(var cmd=new OleDbCommand("SELECT TOP 1 [Id] FROM [Artikulli]",c)){cmd.ExecuteScalar();return true;}}
 }
}

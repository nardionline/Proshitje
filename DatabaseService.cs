using System;
using System.Data;
using System.Data.OleDb;
using System.IO;
namespace ProShitje13 {
 public sealed class DatabaseService : IDisposable {
  private OleDbConnection connection;
  public bool IsConnected => connection != null && connection.State == ConnectionState.Open;
  public void Connect(string path,string password) {
   Dispose();
   if(!File.Exists(path)) throw new FileNotFoundException("Nuk u gjet skedari MDB.",path);
   string cs="Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\""+path.Replace("\"","")+"\";";
   if(!String.IsNullOrEmpty(password)) cs+="Jet OLEDB:Database Password=\""+password.Replace("\"","")+"\";";
   connection=new OleDbConnection(cs); connection.Open();
   using(var cmd=new OleDbCommand("SELECT TOP 1 [Id] FROM [Artikulli]",connection)) cmd.ExecuteScalar();
  }
  public DataTable SearchProducts(string term) {
   if(!IsConnected) throw new InvalidOperationException("Databaza nuk është lidhur.");
   using(var cmd=new OleDbCommand("SELECT TOP 500 [Id],[Kodi],[Barcodi],[Artikulli],[Cmimi i daljes],[Cmimihyrjes],[Sasia Stock] FROM [Artikulli] WHERE [Artikulli] LIKE ? OR [Barcodi] LIKE ? ORDER BY [Artikulli]",connection)) {
    cmd.Parameters.Add("p1",OleDbType.VarWChar,255).Value="%"+term+"%";
    cmd.Parameters.Add("p2",OleDbType.VarWChar,255).Value="%"+term+"%";
    var dt=new DataTable(); using(var da=new OleDbDataAdapter(cmd)) da.Fill(dt); return dt;
   }
  }

  public static DataTable SearchProductsIndependent(string path,string password,string term) {
   if(!File.Exists(path)) throw new FileNotFoundException("Nuk u gjet skedari MDB.",path);
   string cs="Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\""+path.Replace("\"","")+"\";";
   if(!String.IsNullOrEmpty(password)) cs+="Jet OLEDB:Database Password=\""+password.Replace("\"","")+"\";";
   using(var cn=new OleDbConnection(cs)) {
    cn.Open();
    string sql="SELECT TOP 200 [Id],[Kodi],[Barcodi],[Artikulli],[Cmimi i daljes],[Cmimihyrjes],[Sasia Stock] FROM [Artikulli] ";
    if(String.IsNullOrWhiteSpace(term)) sql+="ORDER BY [Artikulli]";
    else sql+="WHERE [Barcodi]=? OR [Kodi]=? OR [Artikulli] LIKE ? OR [Barcodi] LIKE ? OR [Kodi] LIKE ? ORDER BY IIF([Barcodi]=?,0,IIF([Kodi]=?,1,2)), [Artikulli]";
    using(var cmd=new OleDbCommand(sql,cn)) {
     if(!String.IsNullOrWhiteSpace(term)) {
      cmd.Parameters.Add("p1",OleDbType.VarWChar,255).Value=term;
      cmd.Parameters.Add("p2",OleDbType.VarWChar,255).Value=term;
      cmd.Parameters.Add("p3",OleDbType.VarWChar,255).Value="%"+term+"%";
      cmd.Parameters.Add("p4",OleDbType.VarWChar,255).Value="%"+term+"%";
      cmd.Parameters.Add("p5",OleDbType.VarWChar,255).Value="%"+term+"%";
      cmd.Parameters.Add("p6",OleDbType.VarWChar,255).Value=term;
      cmd.Parameters.Add("p7",OleDbType.VarWChar,255).Value=term;
     }
     var dt=new DataTable(); using(var da=new OleDbDataAdapter(cmd)) da.Fill(dt); return dt;
    }
   }
  }
  public void Dispose() { if(connection!=null){connection.Dispose();connection=null;} }
 }
}
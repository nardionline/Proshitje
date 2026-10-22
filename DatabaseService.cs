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
   connection = OpenConnection(path,password);
   using(var cmd=new OleDbCommand("SELECT TOP 1 [Id] FROM [Artikulli]",connection)) cmd.ExecuteScalar();
  }
  static OleDbConnection OpenConnection(string path,string password) {
   if(String.IsNullOrWhiteSpace(path) || !File.Exists(path)) throw new FileNotFoundException("Nuk u gjet skedari MDB.",path);
   string cs="Provider=Microsoft.Jet.OLEDB.4.0;Data Source=\""+path.Replace("\"","")+"\";";
   if(!String.IsNullOrEmpty(password)) cs+="Jet OLEDB:Database Password=\""+password.Replace("\"","")+"\";";
   var c=new OleDbConnection(cs); c.Open(); return c;
  }
  public static DataTable SearchProductsIndependent(string path,string password,string term) {
   using(var c=OpenConnection(path,password)) using(var cmd=new OleDbCommand()) {
    cmd.Connection=c;
    string sql="SELECT TOP 500 [Id],[Kodi],[Barcodi],[Artikulli],[Njësia e matjes],[Cmimi i daljes],[Cmimihyrjes],[Sasia Stock] FROM [Artikulli]";
    if(!String.IsNullOrWhiteSpace(term)) {
     sql += " WHERE [Artikulli] LIKE ? OR [Barcodi] LIKE ? OR [Kodi] LIKE ?";
     cmd.Parameters.Add("p1",OleDbType.VarWChar,255).Value="%"+term.Trim()+"%";
     cmd.Parameters.Add("p2",OleDbType.VarWChar,255).Value="%"+term.Trim()+"%";
     cmd.Parameters.Add("p3",OleDbType.VarWChar,255).Value="%"+term.Trim()+"%";
    }
    sql += " ORDER BY [Artikulli]"; cmd.CommandText=sql;
    var dt=new DataTable(); using(var da=new OleDbDataAdapter(cmd)) da.Fill(dt); return dt;
   }
  }
  public static DataTable ReadTable(string path,string password,string sql) {
   using(var c=OpenConnection(path,password)) using(var cmd=new OleDbCommand(sql,c)) {
    var dt=new DataTable(); using(var da=new OleDbDataAdapter(cmd)) da.Fill(dt); return dt;
   }
  }
  public DataTable SearchProducts(string term) {
   if(!IsConnected) throw new InvalidOperationException("Databaza nuk është lidhur.");
   using(var cmd=new OleDbCommand("SELECT TOP 500 [Id],[Kodi],[Barcodi],[Artikulli],[Njësia e matjes],[Cmimi i daljes],[Cmimihyrjes],[Sasia Stock] FROM [Artikulli] WHERE [Artikulli] LIKE ? OR [Barcodi] LIKE ? OR [Kodi] LIKE ? ORDER BY [Artikulli]",connection)) {
    string q="%"+(term??"")+"%";
    cmd.Parameters.Add("p1",OleDbType.VarWChar,255).Value=q; cmd.Parameters.Add("p2",OleDbType.VarWChar,255).Value=q; cmd.Parameters.Add("p3",OleDbType.VarWChar,255).Value=q;
    var dt=new DataTable(); using(var da=new OleDbDataAdapter(cmd)) da.Fill(dt); return dt;
   }
  }
  public void Dispose() { if(connection!=null){connection.Dispose();connection=null;} }
 }
}

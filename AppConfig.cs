using System;
using System.IO;
using System.Xml.Serialization;
namespace ProShitje13 {
 [Serializable] public class AppConfig {
  public string DatabasePath {get;set;} = "";
  public string DatabasePassword {get;set;} = "";
  public string LicenseSerial {get;set;} = "";
  public string FiscalOutputPath {get;set;} = "";
  public string CompanyName {get;set;} = "ProShitje";
  public string LastRole {get;set;} = "Administrator";
  public static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"ProShitje13","config.xml");
  public static AppConfig Load() {
   try { if(File.Exists(ConfigPath)) using(var f=File.OpenRead(ConfigPath)) return (AppConfig)new XmlSerializer(typeof(AppConfig)).Deserialize(f); }
   catch {}
   return new AppConfig { DatabasePath=Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"marketdb.mdb") };
  }
  public void Save() {
   var dir=Path.GetDirectoryName(ConfigPath); Directory.CreateDirectory(dir);
   using(var f=File.Create(ConfigPath)) new XmlSerializer(typeof(AppConfig)).Serialize(f,this);
  }
 }
}
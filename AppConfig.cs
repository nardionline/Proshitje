using System.Configuration;
namespace ProShitje.Wpf {
 public static class AppConfig {
  public static string AccessPath => ConfigurationManager.AppSettings["AccessPath"] ?? "";
  public static string AccessPassword => ConfigurationManager.AppSettings["AccessPassword"] ?? "";
  public static string FiscalOutputPath => ConfigurationManager.AppSettings["FiscalOutputPath"] ?? @"C:\ProShitje\Fiscal";
  public static string StoreName => ConfigurationManager.AppSettings["StoreName"] ?? "Market";
  public static string CashierName => ConfigurationManager.AppSettings["CashierName"] ?? "Arkëtar";
 }
}

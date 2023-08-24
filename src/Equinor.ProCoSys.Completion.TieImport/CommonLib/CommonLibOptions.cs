//using System.Configuration;

namespace Equinor.ProCoSys.Completion.TieImport.CommonLib;

public class CommonLibOptions
{
    public int CacheDurationDays { get; set; }
    public List<string> SchemaFrom { get; set; } = new();
    public string SchemaTo { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;

    //public MapperSettings()
    //{
        //SchemaFrom = GetSetting("MapperSchemaFrom").Split(','); // comma-separated list
        //SchemaTo = GetSetting("MapperSchemaTo");
        //ClientId = GetSetting("AzureClientId");
        //ClientSecret = GetSetting("MapperClientSecret"); // todo: this probably belongs in a Key vault
        //TenantId = GetSetting("AzureTenantId");
        //CacheDurationDays = int.Parse(GetSetting("MapperCacheDurationDays"));
    //}

    //private static string GetSetting(string appSettingsKey)
    //{
    //    var value = ConfigurationManager.AppSettings[appSettingsKey];

    //    if (string.IsNullOrEmpty(value))
    //    {
    //        throw new Exception($"AppSetting [{appSettingsKey}] is undefined.");
    //    }

    //    return value;
    //}
}

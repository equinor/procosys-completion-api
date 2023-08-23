using System.Configuration;

namespace Equinor.ProCoSys.Completion.TieImport.CommonLib;

public class MapperSettings
{
    public int CacheDurationDays { get; set; }
    public string[] SchemaFrom { get; set; }
    public string SchemaTo { get; set; }
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string TenantId { get; set; }

    public MapperSettings()
    {
        SchemaFrom = GetSetting("MapperSchemaFrom").Split(','); // comma-separated list
        SchemaTo = GetSetting("MapperSchemaTo");
        ClientId = GetSetting("AzureClientId");
        ClientSecret = GetSetting("MapperClientSecret"); // todo: this probably belongs in a Key vault
        TenantId = GetSetting("AzureTenantId");
        CacheDurationDays = int.Parse(GetSetting("MapperCacheDurationDays"));
    }

    private static string GetSetting(string appSettingsKey)
    {
        var value = ConfigurationManager.AppSettings[appSettingsKey];

        if (string.IsNullOrEmpty(value))
        {
            throw new Exception($"AppSetting [{appSettingsKey}] is undefined.");
        }

        return value;
    }
}

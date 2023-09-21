using System.Configuration;

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure;

//TODO: Convert to IOptions instead of ConfigurationManager
public static class ConfigurationHelper
{
    public static long SettingOrDefaultOf(string key, long defaultValue)
    {
        var setting = ConfigurationManager.AppSettings[key];
        return !string.IsNullOrWhiteSpace(setting) ? long.Parse(setting) : defaultValue;
    }

    public static int SettingOrDefaultOf(string key, int defaultValue)
    {
        var setting = ConfigurationManager.AppSettings[key];
        return !string.IsNullOrWhiteSpace(setting) ? int.Parse(setting) : defaultValue;
    }

    public static string SettingOrDefaultOf(string key, string defaultValue)
    {
        var setting = ConfigurationManager.AppSettings[key];
        return !string.IsNullOrWhiteSpace(setting) ? setting : defaultValue;
    }

    public static bool SettingOrDefaultOf(string key, bool defaultValue)
    {
        var setting = ConfigurationManager.AppSettings[key];
        return !string.IsNullOrWhiteSpace(setting) ? bool.Parse(setting) : defaultValue;
    }

    public static string[] SplitSettingOrNone(string key)
    {
        var setting = ConfigurationManager.AppSettings[key];
        return !string.IsNullOrWhiteSpace(setting) ? setting.Split(',') : new string[0];
    }
}

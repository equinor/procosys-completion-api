namespace Equinor.ProCoSys.Completion.TieImport.Extensions;
public static class StringExtensions
{
    public static bool GetValueAsBool(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        return value.Trim().ToUpper() switch
        {
            "Y" => true,
            "YES" => true,
            "JA" => true,
            "J" => true,
            "1" => true,
            "+" => true,
            "TRUE" => true,
            _ => false
        };
    }
}

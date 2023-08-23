namespace Equinor.ProCoSys.Completion.TieImport.Extensions;
public static class StringExtensions
{
    public static bool ContainsAnyOf(this string value, IEnumerable<string> candidates) => value != null && candidates.Any(value.Contains);

    public static bool GetValueAsBool(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        switch (value.Trim().ToUpper())
        {
            case "Y":
            case "YES":
            case "JA":
            case "J":
            case "1":
            case "+":
            case "TRUE":
                return true;
            default:
                return false;
        }
    }
}

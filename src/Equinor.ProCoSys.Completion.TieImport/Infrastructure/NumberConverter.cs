using System.Globalization;

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure;
public class NumberConverter
{
    private static readonly CultureInfo CultureWithComma = new("nb-NO");
    private static readonly CultureInfo CultureWithPoint = new("en-US");

    public static bool IsConvertableToDecimal(string value) => ConvertToDecimal(value) != null;

    //NumberStyles.Float: Indicates that the AllowLeadingWhite, AllowTrailingWhite, AllowLeadingSign, AllowDecimalPoint, and AllowExponent styles are used
    public static decimal? ConvertToDecimal(string value)
        => decimal.TryParse(value, NumberStyles.Float, CultureWithPoint, out var result)
            ? result
            : (decimal.TryParse(value, NumberStyles.Float, CultureWithComma, out result)
                ? result
                : null);
}

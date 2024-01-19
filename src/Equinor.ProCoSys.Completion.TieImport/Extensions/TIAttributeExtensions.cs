//using Statoil.TI.InterfaceServices.Message;
//using System.Globalization;
//using Equinor.ProCoSys.Completion.TieImport.Infrastructure;

//namespace Equinor.ProCoSys.Completion.TieImport.Extensions;
//public static class TIAttributeExtensions
//{
//    public static string? GetValueAsString(this TIAttribute tiAttribute)
//        => string.IsNullOrWhiteSpace(tiAttribute.Value) ? null : tiAttribute.Value.Trim();

//    public static string? GetValueAsStringUpperCase(this TIAttribute tiAttribute)
//        => GetValueAsString(tiAttribute)?.ToUpperInvariant();

//    public static DateTime? GetValueAsDateTime(this TIAttribute tiAttribute)
//        => string.IsNullOrWhiteSpace(tiAttribute.Value)
//            ? null
//            : Convert.ToDateTime(tiAttribute.Value.Trim(), CultureInfo.InvariantCulture);

//    public static bool? GetValueAsBool(this TIAttribute tiAttribute)
//    {
//        var attributeValue = tiAttribute.Value;
//        return string.IsNullOrWhiteSpace(attributeValue) ? null : attributeValue.GetValueAsBool();
//    }

//    public static double? GetValueAsDouble(this TIAttribute tiAttribute)
//    {
//        if (string.IsNullOrWhiteSpace(tiAttribute.Value))
//        {
//            return null;
//        }

//        var toDecimal = NumberConverter.ConvertToDecimal(tiAttribute.Value.Trim());
//        return toDecimal == null ? null : (double)toDecimal;
//    }

//    public static bool HasValue(this TIAttribute tiAttribute)
//        => !string.IsNullOrWhiteSpace(tiAttribute.Value) && !IsBlankingSignal(tiAttribute.Value);

//    public static bool HasBlankingSignal(this TIAttribute tiAttribute)
//        => !string.IsNullOrWhiteSpace(tiAttribute.Value) && IsBlankingSignal(tiAttribute.Value);

//    private static bool IsBlankingSignal(string value)
//        => value.Trim().Equals("{NULL}", StringComparison.InvariantCultureIgnoreCase);
//}

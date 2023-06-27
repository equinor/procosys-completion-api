using System;
using System.Text.RegularExpressions;
using MassTransit;

namespace Equinor.ProCoSys.Completion.Command.MassTransit;

public class KebabCaseEntityNameFormatter : IEntityNameFormatter
{
    private readonly IEntityNameFormatter _defaultFormatter;
    private readonly bool _includeNamespace;

    public KebabCaseEntityNameFormatter(IEntityNameFormatter defaultFormatter, bool includeNamespace)
    {
        _defaultFormatter = defaultFormatter;
        _includeNamespace = includeNamespace;
    }

    public string FormatEntityName<T>()
    {
        var name = _defaultFormatter.FormatEntityName<T>();
        return _includeNamespace ? name.ToKebabCase() : name.Substring(name.LastIndexOf('.') + 1).ToKebabCase();
    }
}

public static class StringExtensions
{
    public static string ToKebabCase(this string str)
    {
        if (string.IsNullOrEmpty(str))
        {
            return str;
        }

        return Regex.Replace(
            str,
            "([a-z0-9])([A-Z])",
            "$1-$2",
            RegexOptions.CultureInvariant,
            TimeSpan.FromMilliseconds(100)).ToLowerInvariant();
    }
}

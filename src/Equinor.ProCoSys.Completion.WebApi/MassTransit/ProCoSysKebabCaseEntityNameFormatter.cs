using System;
using System.Text.RegularExpressions;
using MassTransit;

namespace Equinor.ProCoSys.Completion.WebApi.MassTransit;

public class ProCoSysKebabCaseEntityNameFormatter : IEntityNameFormatter
{
    private readonly string? _prefix;

    public ProCoSysKebabCaseEntityNameFormatter(string? prefix = null) => _prefix = prefix;

    public string FormatEntityName<T>() 
    {
        var name = typeof(T).Name;
        
        // Removing 'IntegrationEvent' suffix if exists
        if (name.EndsWith("IntegrationEvent", StringComparison.Ordinal))
        {
            name = name[..^"IntegrationEvent".Length];
        }

        // Converting to Kebab Case
        name = ConvertToKebabCase(name);
        
        // Adding prefix if exists
        if(_prefix != null)
        {
            name = $"{_prefix}.{name}";
        }
        
        return name;
    }
    
    private static string ConvertToKebabCase(string value)
    {
        value = Regex.Replace(value, @"([a-z0-9])([A-Z])", "$1-$2"); // Split at position where a lower case is followed by upper case
        value = Regex.Replace(value, @"([A-Z])([A-Z][a-z])", "$1-$2"); // Split at position where an upper case is followed by an upper case then a lower case
        value = value.Replace('_', '-'); // Replace underscores with hyphen
        return value.ToLower(); // Convert to lower case
    }
}


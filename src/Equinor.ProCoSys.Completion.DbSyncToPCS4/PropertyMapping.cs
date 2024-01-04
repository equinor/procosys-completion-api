namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Holds configuration of mapping from a source property to a target column. 
 */
public class PropertyMapping(
    string sourcePropertyName,
    PropertyType sourceType,
    string targetColumnName,
    ValueConversion? valueConversionMethod,
    string? targetSequence,
    bool onlyForInsert)
{
    public string SourcePropertyName { get; } = sourcePropertyName;
    public PropertyType SourceType { get; } = sourceType;
    public string TargetColumnName { get; } = targetColumnName;
    public ValueConversion? ValueConversion { get; } = valueConversionMethod;
    public string? TargetSequence { get; } = targetSequence;
    public bool OnlyForInsert { get; } = onlyForInsert;

    /**
    * Helper method to find the value of a configured property in an object. 
    * This value might be in a nested property (e.g ActionBy.Oid)
    */
    public static object? GetSourcePropertyValue(string configuredPropertyName, object sourceObject)
    {
        var sourcePropertyNameParts = configuredPropertyName.Split('.');
        if (sourcePropertyNameParts.Length > 2)
        {
            throw new Exception($"Only one nested level is supported for entities, so {configuredPropertyName} is not supported.");
        }

        var sourcePropertyName = sourcePropertyNameParts[0];
        var sourceProperty = sourceObject.GetType().GetProperty(sourcePropertyName);

        if (sourceProperty == null)
        {
            throw new Exception($"A property in configuration is missing in source object: {configuredPropertyName}");
        }

        var sourcePropertyValue = sourceProperty.GetValue(sourceObject);

        if (sourcePropertyValue != null && sourcePropertyNameParts.Length > 1)
        {
            //We must find the nested property
            sourceProperty = sourcePropertyValue?.GetType().GetProperty(sourcePropertyNameParts[1]);

            if (sourceProperty == null)
            {
                throw new Exception($"A nested property in configuration is missing in source object: {configuredPropertyName}");
            }

            sourcePropertyValue = sourceProperty.GetValue(sourcePropertyValue);
        }

        return sourcePropertyValue;
    }
}

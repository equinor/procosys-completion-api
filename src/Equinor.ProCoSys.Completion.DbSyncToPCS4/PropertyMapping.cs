namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Holds configuration of mapping from a source property to a target column
 */
public class PropertyMapping
{
    public PropertyMapping(
        string sourcePropertyName,
        string targetColumnName,
        PropertyType sourceType,
        ValueConversion? valueConversionMethod)
    {
        SourcePropertyName = sourcePropertyName;
        TargetColumnName = targetColumnName;
        SourceType = sourceType;
        ValueConversion = valueConversionMethod;
    }

    public string SourcePropertyName { get; }
    public string TargetColumnName { get; }
    public PropertyType SourceType { get; }
    public ValueConversion? ValueConversion { get; }
}

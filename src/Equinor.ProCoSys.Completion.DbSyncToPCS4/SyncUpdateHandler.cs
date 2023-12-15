using System.Text;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Executes the an update of a row in the PCS 4 database, based on a sourceObject and mapping configuration
 */
public class SyncUpdateHandler
{
    private readonly IOracleDBExecutor _oracleDBExecutor;

    public SyncUpdateHandler(IOracleDBExecutor oracleDBExecutor)
    {
        _oracleDBExecutor = oracleDBExecutor;
    }

    /**
     * Handle the syncronization
     */
    public async Task<string> BuildSqlUpdateStatementAsync(ISourceObjectMappingConfig sourceObjectMappingConfig, object sourceObject, CancellationToken cancellationToken = default)
    {
        var primaryKeyValue = GetSourcePropertyValue(sourceObjectMappingConfig.PrimaryKey, sourceObject);
        var primaryKeySqlParameterValue = await SqlParameterConversionHelper.GetSqlParameterValueAsync(primaryKeyValue, sourceObjectMappingConfig.PrimaryKey, _oracleDBExecutor, cancellationToken);

        var updatesForTargetColumns = await GetTargetColumnUpdatesAsync(sourceObject, sourceObjectMappingConfig.PropertyMappings, cancellationToken);

        return BuildUpdateStatement(sourceObjectMappingConfig, primaryKeySqlParameterValue, updatesForTargetColumns);
    }


    /**
     * Creates a list with updates the be executed on the target database. 
     */
    private async Task<List<TargetColumnUpdate>> GetTargetColumnUpdatesAsync(object sourceObject, List<PropertyMapping> propertyMappings, CancellationToken cancellationToken)
    {
        var targetColumnUpdates = new List<TargetColumnUpdate>();

        foreach (var propertyMapping in propertyMappings)
        {
            var sourcePropertyValue = GetSourcePropertyValue(propertyMapping, sourceObject);

            var targetColumnValue = await SqlParameterConversionHelper.GetSqlParameterValueAsync(sourcePropertyValue, propertyMapping, _oracleDBExecutor, cancellationToken);

            var columnUpdate = new TargetColumnUpdate(propertyMapping.TargetColumnName, targetColumnValue);

            targetColumnUpdates.Add(columnUpdate);
        }
        return targetColumnUpdates;
    }

    /**
     * Will find the value of the property in given source object. 
     * This value might be in a nested property (e.g ActionBy.Oid)
     */
    private static object? GetSourcePropertyValue(PropertyMapping propertyMapping, object sourceObject)
    {
        var sourcePropertyNameParts = propertyMapping.SourcePropertyName.Split('.');
        if (sourcePropertyNameParts.Length > 2)
        {
            throw new Exception($"Only one nested level is supported for entities, so {propertyMapping.SourcePropertyName} is not supported.");
        }

        var sourcePropertyName = sourcePropertyNameParts[0];
        var sourceProperty = sourceObject.GetType().GetProperty(sourcePropertyName);

        if (sourceProperty == null)
        {
            throw new Exception($"A property in configuration is missing in source object: {propertyMapping.SourcePropertyName}");
        }

        var sourcePropertyValue = sourceProperty.GetValue(sourceObject);

        if (sourcePropertyValue != null && sourcePropertyNameParts.Length > 1)
        {
            //We must find the nested property
            sourceProperty = sourcePropertyValue?.GetType().GetProperty(sourcePropertyNameParts[1]);

            if (sourceProperty == null)
            {
                throw new Exception($"A nested property in configuration is missing in source object: {propertyMapping.SourcePropertyName}");
            }

            sourcePropertyValue = sourceProperty.GetValue(sourcePropertyValue);
        }

        return sourcePropertyValue;
    }

    /**
     * Build a string that gives the update statement
     */
    private static string BuildUpdateStatement(ISourceObjectMappingConfig sourceObjectMappingConfig, string primaryKeySqlParamValue, List<TargetColumnUpdate> updatesForTargetColumns)
    {
        var updateStatement = new StringBuilder($"update {sourceObjectMappingConfig.TargetTable} set ");

        foreach (var columnUpdate in updatesForTargetColumns)
        {
            updateStatement.Append($"{columnUpdate.ColumnName} = {columnUpdate.ColumnValue}");

            if (columnUpdate != updatesForTargetColumns.Last())
            {
                updateStatement.Append(", ");
            }
        }

        //Todo: Vi vil her lage en dictionary med parameterverdier, som senere brukes når vi lager en oracle command.

        updateStatement.Append($" where {sourceObjectMappingConfig.PrimaryKey.TargetColumnName} = {primaryKeySqlParamValue}");

        return updateStatement.ToString();
    }
}

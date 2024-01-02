using System.Text;
using Dapper;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Executes the an update of a row in the PCS 4 database, based on a sourceObject and mapping configuration
 */
public class SqlUpdateStatementBuilder
{
    private readonly IPcs4Repository _oracleDBExecutor;

    public SqlUpdateStatementBuilder(IPcs4Repository oracleDBExecutor)
    {
        _oracleDBExecutor = oracleDBExecutor;
    }

    /**
     * Handle the syncronization
     */
    public async Task<(string sqlStatement, DynamicParameters sqlParameters)> BuildAsync(ISourceObjectMappingConfig sourceObjectMappingConfig, object sourceObject, string plant, CancellationToken cancellationToken = default)
    {
        var primaryKeyValue = GetSourcePropertyValue(sourceObjectMappingConfig.PrimaryKey, sourceObject);
        var primaryKeyTargetValue = await SqlParameterConversionHelper.ConvertSourceValueToTargetValueAsync(primaryKeyValue, sourceObjectMappingConfig.PrimaryKey, plant, _oracleDBExecutor, cancellationToken);

        if (primaryKeyTargetValue == null)
        {
            throw new Exception("Not able to build update statement. Primary key value is null.");
        }

        var updatesForTargetColumns = await GetTargetColumnUpdatesAsync(sourceObject, sourceObjectMappingConfig.PropertyMappings, plant, cancellationToken);

        return BuildUpdateStatement(sourceObjectMappingConfig, primaryKeyTargetValue, updatesForTargetColumns);
    }


    /**
     * Creates a list with updates the be executed on the target database. 
     */
    private async Task<List<TargetColumnData>> GetTargetColumnUpdatesAsync(object sourceObject, List<PropertyMapping> propertyMappings, string plant, CancellationToken cancellationToken)
    {
        var targetColumnUpdates = new List<TargetColumnData>();

        foreach (var propertyMapping in propertyMappings)
        {
            var sourcePropertyValue = GetSourcePropertyValue(propertyMapping, sourceObject);

            var targetColumnValue = await SqlParameterConversionHelper.ConvertSourceValueToTargetValueAsync(sourcePropertyValue, propertyMapping, plant, _oracleDBExecutor, cancellationToken);

            var columnData = new TargetColumnData(propertyMapping.TargetColumnName, targetColumnValue);

            targetColumnUpdates.Add(columnData);
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
    private static (string sqlStatement, DynamicParameters sqlParameters) BuildUpdateStatement(ISourceObjectMappingConfig sourceObjectMappingConfig, object primaryKeyTargetValue, List<TargetColumnData> updatesForTargetColumns)
    {
        var updateStatement = new StringBuilder($"update {sourceObjectMappingConfig.TargetTable} set ");
        var sqlParameters = new DynamicParameters();

        foreach (var columnUpdate in updatesForTargetColumns)
        {
            sqlParameters.Add($":{columnUpdate.ColumnName}", columnUpdate.ColumnValue);
            updateStatement.Append($"{columnUpdate.ColumnName} = :{columnUpdate.ColumnName}");

            if (columnUpdate != updatesForTargetColumns.Last())
            {
                updateStatement.Append(", ");
            }
        }

        var primaryKeyName = sourceObjectMappingConfig.PrimaryKey.TargetColumnName;
        sqlParameters.Add($":{primaryKeyName}", primaryKeyTargetValue);
        updateStatement.Append($" where {primaryKeyName} = :{primaryKeyName}");

        return (updateStatement.ToString(), sqlParameters);
    }
}

using System.Text;
using Dapper;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Executes the an update of a row in the PCS 4 database, based on a sourceObject and mapping configuration
 */
public class SqlUpdateStatementBuilder(IPcs4Repository oracleDBExecutor)
{
    private readonly IPcs4Repository _oracleDBExecutor = oracleDBExecutor;

    /**
     * Handle the syncronization
     */
    public async Task<(string sqlStatement, DynamicParameters sqlParameters)> BuildAsync(ISourceObjectMappingConfig sourceObjectMappingConfig, object sourceObject, string plant, CancellationToken cancellationToken = default)
    {
        var primaryKeyValue = PropertyMapping.GetSourcePropertyValue(sourceObjectMappingConfig.PrimaryKey.SourcePropertyName, sourceObject);
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
            if (propertyMapping.OnlyForInsert)
            {
                continue;
            }

            var sourcePropertyValue = PropertyMapping.GetSourcePropertyValue(propertyMapping.SourcePropertyName, sourceObject);

            var targetColumnValue = await SqlParameterConversionHelper.ConvertSourceValueToTargetValueAsync(sourcePropertyValue, propertyMapping, plant, _oracleDBExecutor, cancellationToken);

            var columnData = new TargetColumnData(propertyMapping.TargetColumnName, targetColumnValue);

            targetColumnUpdates.Add(columnData);
        }
        return targetColumnUpdates;
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

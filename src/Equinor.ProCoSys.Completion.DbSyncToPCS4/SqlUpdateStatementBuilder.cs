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
        var updateStatement = new StringBuilder($"update {sourceObjectMappingConfig.TargetTable} set ");
        var sqlParameters = new DynamicParameters();

        await AddParameters(sourceObjectMappingConfig, sourceObject, plant, updateStatement, sqlParameters, cancellationToken);
        await AddWhereClause(sourceObjectMappingConfig, sourceObject, plant, updateStatement, sqlParameters, cancellationToken);

        return (updateStatement.ToString(), sqlParameters);
    }

    /**
     * Add parameters to the update statement and to the list of dynamic parameters.
     */
    private async Task AddParameters(ISourceObjectMappingConfig sourceObjectMappingConfig, object sourceObject, string plant, StringBuilder updateStatement, DynamicParameters sqlParameters, CancellationToken cancellationToken)
    {
        foreach (var propertyMapping in sourceObjectMappingConfig.PropertyMappings)
        {
            if (propertyMapping.OnlyForInsert)
            {
                continue;
            }

            var sourcePropertyValue = PropertyMapping.GetSourcePropertyValue(propertyMapping.SourcePropertyName, sourceObject);
            var targetColumnValue = await SqlParameterConversionHelper.ConvertSourceValueToTargetValueAsync(sourcePropertyValue, propertyMapping, plant, _oracleDBExecutor, cancellationToken);

            sqlParameters.Add($":{propertyMapping.TargetColumnName}", targetColumnValue);
            updateStatement.Append($"{propertyMapping.TargetColumnName} = :{propertyMapping.TargetColumnName}");

            if (propertyMapping != sourceObjectMappingConfig.PropertyMappings.Last())
            {
                updateStatement.Append(", ");
            }

        }
    }

    /**
     * Add a where clause with the primary key, to the sql statement
     */
    private async Task AddWhereClause(ISourceObjectMappingConfig sourceObjectMappingConfig, object sourceObject, string plant, StringBuilder updateStatement, DynamicParameters sqlParameters, CancellationToken cancellationToken)
    {
        var primaryKeyValue = PropertyMapping.GetSourcePropertyValue(sourceObjectMappingConfig.PrimaryKey.SourcePropertyName, sourceObject);
        var primaryKeyTargetValue = await SqlParameterConversionHelper.ConvertSourceValueToTargetValueAsync(primaryKeyValue, sourceObjectMappingConfig.PrimaryKey, plant, _oracleDBExecutor, cancellationToken);

        if (primaryKeyTargetValue == null)
        {
            throw new Exception("Not able to build update statement. Primary key value is null.");
        }

        var primaryKeyName = sourceObjectMappingConfig.PrimaryKey.TargetColumnName;

        sqlParameters.Add($":{primaryKeyName}", primaryKeyTargetValue);
        updateStatement.Append($" where {primaryKeyName} = :{primaryKeyName}");
    }
}

using System.Text;
using Dapper;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

/**
 * Build sql statement for the deletion of a row in the PCS 4 database, based on a sourceObject and mapping configuration
 */
public class SqlDeleteStatementBuilder(IPcs4Repository pcs4Repository)
{
    public async Task<(string sqlStatement, DynamicParameters sqlParameters)> BuildAsync(
        ISourceObjectMappingConfig sourceObjectMappingConfig,
        object sourceObject,
        string plant,
        CancellationToken cancellationToken)
    {
        var deleteStatement = new StringBuilder($"delete from {sourceObjectMappingConfig.TargetTable} ");

        var primaryKeyValue = PropertyMapping.GetSourcePropertyValue(
            sourceObjectMappingConfig.PrimaryKey.SourcePropertyName,
            sourceObject);

        var primaryKeyTargetValue = await SqlParameterConversionHelper.ConvertSourceValueToTargetValueAsync(
            primaryKeyValue,
            sourceObjectMappingConfig.PrimaryKey,
            plant,
            pcs4Repository,
            cancellationToken);

        if (primaryKeyValue is null || primaryKeyTargetValue is null)
        {
            throw new Exception("Not able to build update statement. Primary key value or primary key target value is null.");
        }

        var primaryKeyName = sourceObjectMappingConfig.PrimaryKey.TargetColumnName;

        var sqlParameters = new DynamicParameters();
        sqlParameters.Add($":{primaryKeyName}", primaryKeyTargetValue);

        deleteStatement.Append($"where {primaryKeyName} = :{primaryKeyName}");

        return (deleteStatement.ToString(), sqlParameters);
    }
}

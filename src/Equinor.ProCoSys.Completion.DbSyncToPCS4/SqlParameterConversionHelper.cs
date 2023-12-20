using Dapper;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public static class SqlParameterConversionHelper
{
    /**
     * Returns a value representing the target parameter value to be included in the sql statement. 
     * If the column config includes a conversion method, this will be used. If not, conversion based on type will be done. 
     */
    public static async Task<object?> ConvertSourceParameterValueToTargetValueAsync(
        object? sourcePropertyValue,
        PropertyMapping propertyMapping,
        IPcs4Repository oracleDBExecutor,
        CancellationToken cancellationToken)
    {
        if (propertyMapping.ValueConversion != null)
        {
            return await ConvertUsingConversionMethodAsync(sourcePropertyValue, propertyMapping.ValueConversion, oracleDBExecutor, cancellationToken);
        }

        return ConvertBasedOnType(sourcePropertyValue, propertyMapping.SourceType);
    }

    /**
     * Returns the value to be used as target value. Values will be converted if necessary, based on type. 
     */
    private static object? ConvertBasedOnType(object? sourcePropertyValue, PropertyType sourcePropertyType)
    {
        switch (sourcePropertyType)
        {
            case PropertyType.Bool:
                if (sourcePropertyValue == null || ((bool)sourcePropertyValue) == false)
                {
                    return "N";
                }
                else
                {
                    return "Y";
                }
            case PropertyType.Guid:
                return GuidToPCS4Guid((Guid?)sourcePropertyValue);
            default:
                return sourcePropertyValue;
        }
    }

    /**
     * Converts the source value to target value based on a conversion method given i mapping configuration.
     */
    public static async Task<object?> ConvertUsingConversionMethodAsync(
        object? sourcePropertyValue,
        ValueConversion? valueConversion,
        IPcs4Repository oracleDBExecutor,
        CancellationToken cancellationToken)
    {
        if (sourcePropertyValue == null)
        {
            return null;
        }

        return valueConversion switch
        {
            ValueConversion.GuidToLibId => await GuidToLibIdAsync((Guid)sourcePropertyValue, oracleDBExecutor, cancellationToken),
            ValueConversion.OidToPersonId => await OidToPersonIdAsync((Guid)sourcePropertyValue, oracleDBExecutor, cancellationToken),
            ValueConversion.GuidToWorkOrderId => await GuidToWorkOrderIdAsync((Guid)sourcePropertyValue, oracleDBExecutor, cancellationToken),
            ValueConversion.GuidToSWCRId => await GuidToSWCRIdAsync((Guid)sourcePropertyValue, oracleDBExecutor, cancellationToken),
            ValueConversion.GuidToDocumentId => await GuidToDocumentIdAsync((Guid)sourcePropertyValue, oracleDBExecutor, cancellationToken),
            _ => throw new NotImplementedException($"Value conversion method {valueConversion}is not implemented."),
        };
    }

    /**
     * Convert a guid to a string that can be used in oracle
     */
    public static string? GuidToPCS4Guid(Guid? guid)
    {
        if (guid == null || !guid.HasValue)
        {
            return null;
        }
        else
        {
            return $"{guid.Value.ToString().Replace("-", string.Empty).ToUpper()}";
        }
    }

    /**
     * Converts a oid (guid) to a string that can be used in Oracle
     */
    public static string? GuidToPCS4Oid(Guid? guid)
    {
        if (guid == null || !guid.HasValue)
        {
            return null;
        }
        else
        {
            return $"{guid.Value.ToString().ToLower()}";
        }
    }

    /**
     * Will find the library id for given guid in the pcs4 database
     */
    private static async Task<long> GuidToLibIdAsync(Guid guid, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlParameters = new DynamicParameters();
        sqlParameters.Add(":procosys_guid", GuidToPCS4Guid(guid));

        var sqlQuery = $"select Library_id from library where procosys_guid = :procosys_guid";

        return await oracleDBExecutor.ValueLookupNumberAsync(sqlQuery, sqlParameters, cancellationToken);
    }

    /**
     * Will find the person_id with given oid in the pcs4 database
     */
    private static async Task<long> OidToPersonIdAsync(Guid oid, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlParameters = new DynamicParameters();
        sqlParameters.Add(":azure_oid", GuidToPCS4Oid(oid));

        var sqlQuery = $"select Person_id from person where azure_oid = :azure_oid";

        return await oracleDBExecutor.ValueLookupNumberAsync(sqlQuery, sqlParameters, cancellationToken);
    }

    /**
     * Will find the work order with given guid in the pcs4 database
     */
    private static async Task<long> GuidToWorkOrderIdAsync(Guid guid, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlParameters = new DynamicParameters();
        sqlParameters.Add(":procosys_guid", GuidToPCS4Guid(guid));

        var sqlQuery = $"select Wo_id from wo where procosys_guid = :procosys_guid";

        return await oracleDBExecutor.ValueLookupNumberAsync(sqlQuery, sqlParameters, cancellationToken);
    }

    /**
     * Will find the SWCR with given guid in the pcs4 database
     */
    private static async Task<long> GuidToSWCRIdAsync(Guid guid, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlParameters = new DynamicParameters();
        sqlParameters.Add(":procosys_guid", GuidToPCS4Guid(guid));

        var sqlQuery = $"select Swcr_id from swcr where procosys_guid = :procosys_guid";

        return await oracleDBExecutor.ValueLookupNumberAsync(sqlQuery, sqlParameters, cancellationToken);
    }

    /**
     * Will find the document with given guid in the pcs4 database
     */
    private static async Task<long> GuidToDocumentIdAsync(Guid guid, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlParameters = new DynamicParameters();
        sqlParameters.Add(":procosys_guid", GuidToPCS4Guid(guid));

        var sqlQuery = $"select Document_id from document where procosys_guid = :procosys_guid";

        return await oracleDBExecutor.ValueLookupNumberAsync(sqlQuery, sqlParameters, cancellationToken);
    }
}

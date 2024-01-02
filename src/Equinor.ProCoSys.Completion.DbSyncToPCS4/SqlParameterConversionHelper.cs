using Dapper;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public static class SqlParameterConversionHelper
{
    /**
     * Returns a value representing the target parameter value to be included in the sql statement, based on source property value
     * If the column config includes a conversion method, this will be used. If not, conversion based on type will be done. 
     */
    public static async Task<object?> ConvertSourceValueToTargetValueAsync(
        object? value,
        PropertyMapping propertyMapping,
        string plant,
        IPcs4Repository oracleDBExecutor,
        CancellationToken cancellationToken)
    {
        if (value == null)
        {
            return null;
        }

        if (propertyMapping.ValueConversion != null)
        {
            return propertyMapping.ValueConversion switch
            {
                ValueConversion.GuidToLibId => await GuidToLibIdAsync((Guid)value, oracleDBExecutor, cancellationToken),
                ValueConversion.OidToPersonId => await OidToPersonIdAsync((Guid)value, oracleDBExecutor, cancellationToken),
                ValueConversion.GuidToWorkOrderId => await GuidToWorkOrderIdAsync((Guid)value, oracleDBExecutor, cancellationToken),
                ValueConversion.GuidToSWCRId => await GuidToSWCRIdAsync((Guid)value, oracleDBExecutor, cancellationToken),
                ValueConversion.GuidToDocumentId => await GuidToDocumentIdAsync((Guid)value, oracleDBExecutor, cancellationToken),
                ValueConversion.GuidToTagCheckId => await GuidToTagCheckIdAsync((Guid)value, oracleDBExecutor, cancellationToken),
                ValueConversion.PunchCategoryToLibId => await PunchCategoryToLibIdAsync((string)value, plant, oracleDBExecutor, cancellationToken),
                _ => throw new NotImplementedException($"Value conversion method {propertyMapping.ValueConversion}is not implemented."),
            };
        }

        return ConvertBasedOnType(value, propertyMapping.SourceType);
    }

    /**
     * Returns the value to be used as target value. Values will be converted if necessary, based on type. 
     */
    private static object ConvertBasedOnType(object sourcePropertyValue, PropertyType sourcePropertyType)
    {
        switch (sourcePropertyType)
        {
            case PropertyType.Bool:
                if ((bool)sourcePropertyValue == false)
                {
                    return "N";
                }
                else
                {
                    return "Y";
                }
            case PropertyType.Guid:
                return GuidToPCS4Guid((Guid)sourcePropertyValue);
            default:
                return sourcePropertyValue;
        }
    }

    /**
     * Convert a guid to a string that can be used in oracle
     */
    public static string GuidToPCS4Guid(Guid guid) => $"{guid.ToString().Replace("-", string.Empty).ToUpper()}";

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

    /**
     * Will find the tag check with given guid in the pcs4 database
     */
    private static async Task<long> GuidToTagCheckIdAsync(Guid guid, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlParameters = new DynamicParameters();
        sqlParameters.Add(":procosys_guid", GuidToPCS4Guid(guid));

        var sqlQuery = $"select TagCheck_id from TagCheck where procosys_guid = :procosys_guid";

        return await oracleDBExecutor.ValueLookupNumberAsync(sqlQuery, sqlParameters, cancellationToken);
    }

    /**
    * Will find the tag check with given guid in the pcs4 database
    */
    private static async Task<long> PunchCategoryToLibIdAsync(string punchCategory, string plant, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlParameters = new DynamicParameters();
        sqlParameters.Add(":code", punchCategory);
        sqlParameters.Add(":projectSchema", plant);

        var sqlQuery = $"select library_id from library where librarytype = 'COMPLETION_STATUS' and code = '{punchCategory}' and projectSchema = '{plant}'";

        return await oracleDBExecutor.ValueLookupNumberAsync(sqlQuery, sqlParameters, cancellationToken);
    }
}

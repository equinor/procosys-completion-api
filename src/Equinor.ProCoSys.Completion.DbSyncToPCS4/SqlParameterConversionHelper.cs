namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public static class SqlParameterConversionHelper
{
    /**
     * Returns a string representing the target sql parameter value to be included in the sql statement. 
     * If the column config includes a conversion method, this will be used. If not, conversion based on type will be done. 
     */
    public static async Task<string> GetSqlParameterValueAsync(
        object? sourcePropertyValue,
        PropertyMapping propertyMapping,
        IPcs4Repository oracleDBExecutor,
        CancellationToken cancellationToken)
    {
        if (propertyMapping.ValueConversion != null)
        {
            return await GetSqlParameterValueUsingConversionMethodAsync(sourcePropertyValue, propertyMapping.ValueConversion, oracleDBExecutor, cancellationToken);
        }

        return GetSqlParameterValueBasedOnType(sourcePropertyValue, propertyMapping.SourceType);
    }

    /**
     * Returns a string that can be used as sql parameter value, based on type
     */
    private static string GetSqlParameterValueBasedOnType(object? sourcePropertyValue, PropertyType sourcePropertyType)
    {
        switch (sourcePropertyType)
        {
            case PropertyType.String:
                return sourcePropertyValue != null ? $"'{sourcePropertyValue}'" : "null";
            case PropertyType.Int:
                return sourcePropertyValue != null ? $"{sourcePropertyValue}" : "null";
            case PropertyType.Bool:
                if (sourcePropertyValue == null || ((bool)sourcePropertyValue) == false)
                {
                    return "'N'";
                }
                else
                {
                    return "'Y'";
                }
            case PropertyType.DateTime:
                if (sourcePropertyValue == null)
                {
                    return "null";
                }
                var dateTime = (DateTime)sourcePropertyValue;
                return $"to_date('{dateTime.Day}/{dateTime.Month}/{dateTime.Year} {dateTime.Hour}:{dateTime.Minute}:{dateTime.Second}', 'DD/MM/YYYY HH24:MI:SS')";
            case PropertyType.Guid:
                return GuidToPCS4Guid((Guid?)sourcePropertyValue);
            default:
                throw new NotImplementedException();
        }
    }

    /**
     * Converts a value to an sql parameter string based on a conversion method given i mapping configuration.
     */
    public static async Task<string> GetSqlParameterValueUsingConversionMethodAsync(
        object? sourcePropertyValue,
        ValueConversion? valueConversion,
        IPcs4Repository oracleDBExecutor,
        CancellationToken cancellationToken)
    {
        if (sourcePropertyValue == null)
        {
            return "null";
        }

        return valueConversion switch
        {
            ValueConversion.GuidToLibId => await GuidToLibIdAsync((Guid)sourcePropertyValue, oracleDBExecutor, cancellationToken),
            ValueConversion.OidToPersonId => await OidToPersonIdAsync((Guid)sourcePropertyValue, oracleDBExecutor, cancellationToken),
            ValueConversion.GuidToWorkOrderId => await GuidToWorkOrderIdAsync((Guid)sourcePropertyValue, oracleDBExecutor, cancellationToken),
            ValueConversion.GuidToSWCRId => await GuidToSWCRIdAsync((Guid)sourcePropertyValue, oracleDBExecutor, cancellationToken),
            ValueConversion.GuidToDocumentId => await GuidToDocumentIdAsync((Guid)sourcePropertyValue, oracleDBExecutor, cancellationToken),
            ValueConversion.DateTimeToDate => DateTimeToDate((DateTime)sourcePropertyValue),
            _ => throw new NotImplementedException($"Value conversion method {valueConversion}is not implemented."),
        };
    }

    /**
     * Convert a guid to a string that can be used in oracle
     */
    public static string GuidToPCS4Guid(Guid? guid)
    {
        if (guid == null || !guid.HasValue)
        {
            return "null";
        }
        else
        {
            return $"'{guid.Value.ToString().Replace("-", string.Empty).ToUpper()}'";
        }
    }

    /**
     * Converts a oid (guid) to a string that can be used in Oracle
     */
    public static string GuidToPCS4Oid(Guid? guid)
    {
        if (guid == null || !guid.HasValue)
        {
            return "null";
        }
        else
        {
            return $"'{guid.Value.ToString().ToLower()}'";
        }
    }

    /**
     * Will find the library id for given guid in the pcs4 database
     */
    private static async Task<string> GuidToLibIdAsync(Guid guid, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlQuery = $"select library_id from library where procosys_guid = {GuidToPCS4Guid(guid)}";

        var libraryId = await oracleDBExecutor.ValueLookupAsync(sqlQuery, cancellationToken);
        if (libraryId != null)
        {
            return libraryId;
        }
        throw new Exception($"Not able to find a library item in pcs4 with guid = {guid}");
    }

    /**
     * Will find the person_id with given oid in the pcs4 database
     */
    private static async Task<string> OidToPersonIdAsync(Guid oid, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlQuery = $"select person_id from person where azure_oid = {GuidToPCS4Oid(oid)}";

        var personId = await oracleDBExecutor.ValueLookupAsync(sqlQuery, cancellationToken);
        if (personId != null)
        {
            return personId;
        }
        throw new Exception($"Not able to find a person in pcs4 with azure_oid = {oid}");
    }

    /**
     * Will find the work order with given guid in the pcs4 database
     */
    private static async Task<string> GuidToWorkOrderIdAsync(Guid guid, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlQuery = $"select wo_id from wo where procosys_guid = {GuidToPCS4Guid(guid)}";

        var woId = await oracleDBExecutor.ValueLookupAsync(sqlQuery, cancellationToken);
        if (woId != null)
        {
            return woId;
        }
        throw new Exception($"Not able to find work order in pcs4 with guid = {guid}");
    }

    /**
     * Will find the SWCR with given guid in the pcs4 database
     */
    private static async Task<string> GuidToSWCRIdAsync(Guid guid, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlQuery = $"select swcr_id from swcr where procosys_guid = {GuidToPCS4Guid(guid)}";

        var swcrId = await oracleDBExecutor.ValueLookupAsync(sqlQuery, cancellationToken);
        if (swcrId != null)
        {
            return swcrId;
        }
        throw new Exception($"Not able to find SWCR in pcs4 with guid = {guid}");
    }

    /**
     * Will find the document with given guid in the pcs4 database
     */
    private static async Task<string> GuidToDocumentIdAsync(Guid guid, IPcs4Repository oracleDBExecutor, CancellationToken cancellationToken)
    {
        var sqlQuery = $"select document_id from document where procosys_guid = {GuidToPCS4Guid(guid)}";

        var documentId = await oracleDBExecutor.ValueLookupAsync(sqlQuery, cancellationToken);
        if (documentId != null)
        {
            return documentId;
        }
        throw new Exception($"Not able to find document in pcs4 with guid = {guid}");
    }

    /**
     * Converts a datetime to a date (time is not included)
     */
    private static string DateTimeToDate(DateTime date)
    {
        return $"to_date('{date.Day}/{date.Month}/{date.Year}', 'DD/MM/YYYY')";
    }
}

using static Equinor.ProCoSys.Completion.DbSyncToPCS4.SyncMappingConfig;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public static class ValueConversion
    {
        /**
         * Returns a string representing the target sql parameter value to be included in the sql statement. 
         * If the column config includes a conversion method, this will be used. If not, standard conversion will be used. 
         */
        public static async Task<string> GetSqlParameterValueAsync(object? sourceValue, ColumnSyncConfig columnConfig, IOracleDBExecutor oracleDBExecutor, CancellationToken cancellationToken)
        {
            if (columnConfig.ValueConversionMethod != null)
            {
                return await ConvertToSqlParameterValueAsync(sourceValue, columnConfig.ValueConversionMethod, oracleDBExecutor, cancellationToken);
            }

            switch (columnConfig.SourceType)
            {
                case DataColumnType.String:
                    return sourceValue != null ? $"'{sourceValue}'" : "null";
                case DataColumnType.Int:
                    return sourceValue != null ? $"{sourceValue}" : "null";
                case DataColumnType.Bool:
                    if (sourceValue == null || sourceValue.ToString() == "0")
                    {
                        return "'N'";
                    }
                    else
                    {
                        return "'Y'";
                    }
                case DataColumnType.DateTime:
                    if (sourceValue == null)
                    {
                        return "null";
                    }
                    var dateTime = (DateTime)sourceValue;
                    return $"to_date('{dateTime.Day}/{dateTime.Month}/{dateTime.Year} {dateTime.Hour}:{dateTime.Minute}:{dateTime.Second}', 'DD/MM/YYYY HH24:MI:SS')";
                case DataColumnType.Guid:
                    return GuidToPCS4Guid((Guid?)sourceValue);
                default:
                    throw new NotImplementedException();
            }
        }

        /**
         * Converts a source value to a target sql parameter value based on a conversion method given i mapping configuraiton.
         */
        public static async Task<string> ConvertToSqlParameterValueAsync(object? sourceValue, string conversionMethod, IOracleDBExecutor oracleDBExecutor, CancellationToken cancellationToken)
        {
            if (sourceValue == null)
            {
                return "null";
            }

            return conversionMethod switch
            {
                "GuidToLibId" => await GuidToLibIdAsync((Guid)sourceValue, oracleDBExecutor, cancellationToken),
                "OidToPersonId" => await ValueConversion.OidToPersonIdAsync((Guid)sourceValue, oracleDBExecutor, cancellationToken),
                "GuidToWorkOrderId" => await ValueConversion.GuidToWorkOrderIdAsync((Guid)sourceValue, oracleDBExecutor, cancellationToken),
                "GuidToSWCRId" => await ValueConversion.GuidToSWCRIdAsync((Guid)sourceValue, oracleDBExecutor, cancellationToken),
                "GuidToDocumentId" => await ValueConversion.GuidToDocumentIdAsync((Guid)sourceValue, oracleDBExecutor, cancellationToken),
                "DateTimeToDate" => ValueConversion.DateTimeToDate((DateTime)sourceValue),
                _ => throw new NotImplementedException($"Conversion method {conversionMethod}is not implemented."),
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
        public static async Task<string> GuidToLibIdAsync(Guid guid, IOracleDBExecutor oracleDBExecutor, CancellationToken cancellationToken)
        {
            var sqlQuery = $"select library_id from library where procosys_guid = {GuidToPCS4Guid(guid)}";

            var libraryId = await oracleDBExecutor.ExecuteDBQueryForValueLookupAsync(sqlQuery, cancellationToken);
            if (libraryId != null)
            {
                return libraryId;
            }
            throw new Exception($"Not able to find a library item in pcs4 with guid = {guid}");
        }

        /**
         * Will find the person_id with given oid in the pcs4 database
         */
        public static async Task<string> OidToPersonIdAsync(Guid oid, IOracleDBExecutor oracleDBExecutor, CancellationToken cancellationToken)
        {
            var sqlQuery = $"select person_id from person where azure_oid = {GuidToPCS4Oid(oid)}";

            var personId = await oracleDBExecutor.ExecuteDBQueryForValueLookupAsync(sqlQuery, cancellationToken);
            if (personId != null)
            {
                return personId;
            }
            throw new Exception($"Not able to find a person in pcs4 with azure_oid = {oid}");
        }

        /**
         * Will find the work order with given guid in the pcs4 database
         */
        public static async Task<string> GuidToWorkOrderIdAsync(Guid guid, IOracleDBExecutor oracleDBExecutor, CancellationToken cancellationToken)
        {
            var sqlQuery = $"select wo_id from wo where procosys_guid = {GuidToPCS4Guid(guid)}";

            var woId = await oracleDBExecutor.ExecuteDBQueryForValueLookupAsync(sqlQuery, cancellationToken);
            if (woId != null)
            {
                return woId;
            }
            throw new Exception($"Not able to find work order in pcs4 with guid = {guid}");
        }

        /**
         * Will find the SWCR with given guid in the pcs4 database
         */
        public static async Task<string> GuidToSWCRIdAsync(Guid guid, IOracleDBExecutor oracleDBExecutor, CancellationToken cancellationToken)
        {
            var sqlQuery = $"select swcr_id from swcr where procosys_guid = {GuidToPCS4Guid(guid)}";

            var swcrId = await oracleDBExecutor.ExecuteDBQueryForValueLookupAsync(sqlQuery, cancellationToken);
            if (swcrId != null)
            {
                return swcrId;
            }
            throw new Exception($"Not able to find SWCR in pcs4 with guid = {guid}");
        }

        /**
         * Will find the document with given guid in the pcs4 database
         */
        public static async Task<string> GuidToDocumentIdAsync(Guid guid, IOracleDBExecutor oracleDBExecutor, CancellationToken cancellationToken)
        {
            var sqlQuery = $"select document_id from document where procosys_guid = {GuidToPCS4Guid(guid)}";

            var documentId = await oracleDBExecutor.ExecuteDBQueryForValueLookupAsync(sqlQuery, cancellationToken);
            if (documentId != null)
            {
                return documentId;
            }
            throw new Exception($"Not able to find document in pcs4 with guid = {guid}");
        }

        /**
         * Converts a datetime to a date (time is not included)
         */
        public static string DateTimeToDate(DateTime date)
        {
            return $"to_date('{date.Day}/{date.Month}/{date.Year}', 'DD/MM/YYYY')";
        }
    }
}

using static Equinor.ProCoSys.Completion.DbSyncToPCS4.SyncMappingConfig;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public static class ValueConvertion
    {

        /**
         * Returns a string representing the target sql parameter value to be included in the sql statement. 
         * If the column config includes a convertion method, this will be used. If not, standard convertion will be used. 
         */
        public static async Task<string> GetSqlParameterValue(object? sourceValue, ColumnSyncConfig columnConfig, IOracleDBExecutor oracleDBExecutor)
        {
            if (columnConfig.ValueConvertionMethod != null)
            {
                return await ConvertToSqlParameterValue(sourceValue, columnConfig.ValueConvertionMethod, oracleDBExecutor);
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
         * Converts a source value to a target sql parameter value based on a convertion method given i mapping configuraiton.
         */
        public static async Task<string> ConvertToSqlParameterValue(object? sourceValue, string convertionMethod, IOracleDBExecutor oracleDBExecutor)
        {
            if (sourceValue == null)
            {
                return "null";
            }

            switch (convertionMethod)
            {
                case "GuidToLibId":
                    return await GuidToLibId((Guid)sourceValue, oracleDBExecutor);
                case "GuidToPersonId":
                    return await ValueConvertion.GuidToPersonId((Guid)sourceValue, oracleDBExecutor);
                case "GuidToWorkOrderId":
                    return await ValueConvertion.GuidToWorkOrderId((Guid)sourceValue, oracleDBExecutor);
                case "GuidToSWCRId":
                    return await ValueConvertion.GuidToSWCRId((Guid)sourceValue, oracleDBExecutor);
                case "GuidToDocumentId":
                    return await ValueConvertion.GuidToDocumentId((Guid)sourceValue, oracleDBExecutor);
                case "DateTimeToDate":
                    return ValueConvertion.DateTimeToDate((DateTime)sourceValue);
                default:
                    throw new NotImplementedException($"Convertion method {convertionMethod}is not implemented.");
            }
        }

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
        public static async Task<string> GuidToLibId(Guid guid, IOracleDBExecutor oracleDBExecutor)
        {
            var sqlQuery = $"select library_id from library where procosys_guid = {GuidToPCS4Guid(guid)}";

            var libraryId = await oracleDBExecutor.ExecuteDBQueryForValueLookup(sqlQuery);
            if (libraryId != null)
            {
                return libraryId;
            }
            throw new Exception($"Not able to find a library item in pcs4 with guid = {guid}");
        }

        /**
         * Will find the person_id with given guid in the pcs4 database
         */
        public static async Task<string> GuidToPersonId(Guid guid, IOracleDBExecutor oracleDBExecutor)
        {
            var sqlQuery = $"select person_id from person where azure_oid = {GuidToPCS4Oid(guid)}";

            var personId = await oracleDBExecutor.ExecuteDBQueryForValueLookup(sqlQuery);
            if (personId != null)
            {
                return personId;
            }
            throw new Exception($"Not able to find a person in pcs4 with azure_oid = {guid}");
        }

        /**
         * Will find the work order with given guid in the pcs4 database
         */
        public static async Task<string> GuidToWorkOrderId(Guid guid, IOracleDBExecutor oracleDBExecutor)
        {
            var sqlQuery = $"select wo_id from wo where procosys_guid = {GuidToPCS4Guid(guid)}";

            var woId = await oracleDBExecutor.ExecuteDBQueryForValueLookup(sqlQuery);
            if (woId != null)
            {
                return woId;
            }
            throw new Exception($"Not able to find work order in pcs4 with guid = {guid}");
        }

        /**
         * Will find the SWCR with given guid in the pcs4 database
         */
        public static async Task<string> GuidToSWCRId(Guid guid, IOracleDBExecutor oracleDBExecutor)
        {
            var sqlQuery = $"select swcr_id from swcr where procosys_guid = {GuidToPCS4Guid(guid)}";

            var swcrId = await oracleDBExecutor.ExecuteDBQueryForValueLookup(sqlQuery);
            if (swcrId != null)
            {
                return swcrId;
            }
            throw new Exception($"Not able to find SWCR in pcs4 with guid = {guid}");
        }

        /**
         * Will find the document with given guid in the pcs4 database
         */
        public static async Task<string> GuidToDocumentId(Guid guid, IOracleDBExecutor oracleDBExecutor)
        {
            var sqlQuery = $"select document_id from document where procosys_guid = {GuidToPCS4Guid(guid)}";

            var documentId = await oracleDBExecutor.ExecuteDBQueryForValueLookup(sqlQuery);
            if (documentId != null)
            {
                return documentId;
            }
            throw new Exception($"Not able to find document in pcs4 with guid = {guid}");
        }

        public static string DateTimeToDate(DateTime date)
        {
            return $"to_date('{date.Day}/{date.Month}/{date.Year}', 'DD/MM/YYYY')";
        }
    }
}

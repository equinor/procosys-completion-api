using static Equinor.ProCoSys.Completion.DbSyncToPCS4.Column;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public static class ValueConvertion
    {

        /**
         * Will find the library id for given guid, in the pcs4 database
         */
        public static async Task<string> GuidToMainLibId(string guid, IOracleDBExecutor oracleDBExecutor)
        {
            string pcs4Guid = $"'{guid.Replace("-", string.Empty).ToUpper()}'";
            var sqlQuery = $"select library_id from library where procosys_guid = {pcs4Guid}";

            var libraryId = await oracleDBExecutor.ExecuteDBQueryForValueLookup(sqlQuery);
            if (libraryId != null)
            {
                return libraryId;
            }

            //kast exception eller log
            throw new Exception($"Not able to find a Tagcheck in main with procosys_guid = {pcs4Guid}");
        }

        /**
         * Returns a string representing the value to be included in the sql statement. 
         */
        public static string GetSqlParameterValue(string? value, DataType type)
        {
            switch (type)
            {
                case DataType.String:
                    return $"'{value}'";
                case DataType.Int:
                    return value ?? ""; //todo
                case DataType.Date:
                    return $"'{value}'"; //todo
                case DataType.Guid:
                    return value == null ? "''" : $"'{value.Replace("-", string.Empty).ToUpper()}'";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

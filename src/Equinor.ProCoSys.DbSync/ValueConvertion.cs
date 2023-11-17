using Oracle.ManagedDataAccess.Client;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public static class ValueConvertion
    {

        /**
         * Will find the library id for given guid, in the pcs4 database
         */
        public static string GuidToMainLibId(string procosys_guid, string dbConnStr)
        {
            var sqlQuery = $"select library_id from library where procosys_guid = {procosys_guid}";

            var libraryId = ExecuteDBQueryForValueLookup(sqlQuery, dbConnStr);
            if (libraryId != null)
            {
                return libraryId;
            }

            //kast exception eller log
            throw new Exception($"Not able to find a Tagcheck in main with procosys_guid = {procosys_guid}");
        }

        private static string? ExecuteDBQueryForValueLookup(string sqlQuery, string dbConnStr)
        {
            using (OracleConnection connection = new OracleConnection(dbConnStr))
            {
                connection.Open();

                using (OracleCommand command = new OracleCommand(sqlQuery, connection))
                {
                    using (OracleDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            //todo: Bør kanskje sjekke at vi bare for returnert én rad
                            var value = Convert.ToString(command.ExecuteScalar());
                            if (value != null)
                            {
                                return value;
                            }
                        }
                        return null;
                    }
                }
            }
        }

    }
}

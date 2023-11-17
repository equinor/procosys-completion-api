using System;
using System.Reflection.Metadata.Ecma335;

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public class Column
    {

        public enum DataType
        {
            String,
            Int,
            Date,
            Guid
        }

        public DataType Type { get; set; } = DataType.String;
        public required string Name { get; set; }
        public required string? Value { get; set; }
    }
}

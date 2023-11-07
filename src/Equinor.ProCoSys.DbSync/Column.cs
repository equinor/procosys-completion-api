using System;
using System.Reflection.Metadata.Ecma335;

namespace Equinor.ProCoSys.DbSyncPOC
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

        public String getSqlParameter()
        {
            switch (Type)
            {
                case DataType.String:
                    return $"'{Value}'";
                case DataType.Int:
                    return Value ?? ""; //todo
                case DataType.Date:
                    return $"'{Value}'"; //todo
                case DataType.Guid:
                    return Value == null ? "''" : $"'{Value.Replace("-", string.Empty).ToUpper()}'";
                default:
                    throw new NotImplementedException();
            }
        }
    }
}

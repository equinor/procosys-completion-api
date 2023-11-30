namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public interface ISyncMappingConfig
{
    List<ColumnSyncConfig> GetSyncMappingsForSourceObject(string sourceObjectName);
}
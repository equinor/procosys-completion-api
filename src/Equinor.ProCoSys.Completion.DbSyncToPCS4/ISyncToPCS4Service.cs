namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public interface ISyncToPCS4Service
{
    Task SyncUpdatesAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken = default);
    Task SyncInsertAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken = default);
}

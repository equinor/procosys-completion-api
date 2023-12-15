namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public interface ISyncToPCS4Service
{
    Task SyncUpdatesAsync(string sourceObjectName, object sourceObject, CancellationToken token = default);
}

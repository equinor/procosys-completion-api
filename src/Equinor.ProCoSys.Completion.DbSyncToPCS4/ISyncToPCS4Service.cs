namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public interface ISyncToPCS4Service
{
    Task SyncNewObjectAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken = default);
    Task SyncObjectUpdateAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken = default);
    Task SyncObjectDeletionAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken = default);
}

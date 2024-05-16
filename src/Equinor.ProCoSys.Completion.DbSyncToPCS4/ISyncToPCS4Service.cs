namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public interface ISyncToPCS4Service
{
    Task SyncNewObjectAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken);
    Task SyncObjectUpdateAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken);
    Task SyncObjectDeletionAsync(string sourceObjectName, object sourceObject, string plant, CancellationToken cancellationToken);

    Task SyncNewPunchListItemAsync(object updateEvent, CancellationToken cancellationToken);
    Task SyncPunchListItemUpdateAsync(object updateEvent, CancellationToken cancellationToken);
    Task SyncPunchListItemDeleteAsync(object deleteEvent, CancellationToken cancellationToken);
}

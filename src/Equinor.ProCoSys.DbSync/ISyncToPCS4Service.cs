namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public interface ISyncToPCS4Service
    {
        Task SyncUpdates(string sourceObjectName, object sourceObject, CancellationToken token = default);
    }
}

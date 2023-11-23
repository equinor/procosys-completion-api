namespace Equinor.ProCoSys.Completion.DbSyncToPCS4
{
    public interface ISyncToPCS4Service
    {
        Task SyncUpdates(object integrationEntity, CancellationToken token = default);
    }
}

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;

public interface ISyncTokenService
{
    Task<string> GetAccessTokenAsync();
}

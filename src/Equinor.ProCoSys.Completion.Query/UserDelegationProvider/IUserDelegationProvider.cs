using Azure.Storage.Blobs.Models;

namespace Equinor.ProCoSys.Completion.Query.UserDelegationProvider;

public interface IUserDelegationProvider
{
    public UserDelegationKey GetUserDelegationKey();
}

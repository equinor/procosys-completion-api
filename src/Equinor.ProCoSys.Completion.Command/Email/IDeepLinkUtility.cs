using System;

namespace Equinor.ProCoSys.Completion.Command.Email;

public interface IDeepLinkUtility
{
    string CreateUrl(string typeName, Guid guid);
}

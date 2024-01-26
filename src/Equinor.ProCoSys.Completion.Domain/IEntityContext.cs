using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain;

public interface IEntityContext : IHaveGuid
{
    string GetContextType();
    dynamic GetEmailContext();
}

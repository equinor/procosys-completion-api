using System;

namespace Equinor.ProCoSys.Completion.Domain;

// abstract class to be used for all MediatR commands (write) and queries (read), where user need access to project
public abstract class NeedProjectAccess
{
    public abstract Guid GetProjectGuidForAccessCheck();
}

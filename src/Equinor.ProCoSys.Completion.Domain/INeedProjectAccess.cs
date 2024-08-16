using System;

namespace Equinor.ProCoSys.Completion.Domain;

// interface to be used for all MediatR commands (write) and queries (read), where user need access to project
public interface INeedProjectAccess
{
    public Guid GetProjectGuidForAccessCheck();
}

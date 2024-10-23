using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.Domain;

// interface to be used for all MediatR commands (write) and queries (read), where user need access to several projects
public interface INeedProjectsAccess
{
    public IEnumerable<Guid> GetProjectGuidsForAccessCheck();
}

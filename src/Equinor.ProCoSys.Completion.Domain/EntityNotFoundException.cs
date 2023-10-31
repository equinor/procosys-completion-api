using System;

namespace Equinor.ProCoSys.Completion.Domain;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException()
    {
    }
    public EntityNotFoundException(string message) : base(message)
    {
    }
}

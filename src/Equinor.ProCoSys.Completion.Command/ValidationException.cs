using System;

namespace Equinor.ProCoSys.Completion.Command;

public class ValidationException : Exception
{
    public ValidationException(string error) : base(error)
    {
    }
}

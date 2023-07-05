using System;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public class InValidProjectException : Exception
{
    public InValidProjectException(string error) : base(error)
    {
    }
}
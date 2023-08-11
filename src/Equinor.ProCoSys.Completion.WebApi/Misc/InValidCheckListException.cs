using System;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public class InValidCheckListException : Exception
{
    public InValidCheckListException(string error) : base(error)
    {
    }
}

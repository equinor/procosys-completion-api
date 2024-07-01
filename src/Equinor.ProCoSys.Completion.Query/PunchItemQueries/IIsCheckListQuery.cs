using MediatR;
using System;

namespace Equinor.ProCoSys.Completion.Query.PunchItemQueries;

public interface IIsCheckListQuery : IBaseRequest
{
    Guid CheckListGuid { get; }
}

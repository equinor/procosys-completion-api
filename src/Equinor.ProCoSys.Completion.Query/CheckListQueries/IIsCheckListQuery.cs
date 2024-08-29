using Equinor.ProCoSys.Completion.Domain;
using MediatR;
using System;

namespace Equinor.ProCoSys.Completion.Query.CheckListQueries;

public interface IIsCheckListQuery : INeedProjectAccess, IBaseRequest
{
    Guid CheckListGuid { get; }
    CheckListDetailsDto CheckListDetailsDto { get; set; }
}

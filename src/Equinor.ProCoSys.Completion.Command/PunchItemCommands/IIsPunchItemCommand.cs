using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands;

public interface IIsPunchItemCommand : IBaseRequest
{
    Guid PunchItemGuid { get; }
    PunchItem PunchItem { get; set; }
}

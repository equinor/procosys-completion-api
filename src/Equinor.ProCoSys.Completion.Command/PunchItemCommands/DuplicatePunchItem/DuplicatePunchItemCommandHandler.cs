using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;
using Microsoft.Extensions.Logging;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DuplicatePunchItem;

public class DuplicatePunchItemCommandHandler(
    //ISyncToPCS4Service syncToPCS4Service,
    //IUnitOfWork unitOfWork,
    //IMessageProducer messageProducer,
    //ICheckListApiService checkListApiService,
    //ILogger<DuplicatePunchItemCommandHandler> logger
    )
    : PunchUpdateCommandBase, IRequestHandler<DuplicatePunchItemCommand, Result<List<GuidAndRowVersion>>>
{
    public Task<Result<List<GuidAndRowVersion>>> Handle(DuplicatePunchItemCommand request, CancellationToken cancellationToken)
    {
        var punchItem = request.PunchItem;

        throw new NotImplementedException();
    }
}

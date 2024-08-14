using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Behaviors;

public class PrefetchBehavior<TRequest, TResponse>(
    ILogger<PrefetchBehavior<TRequest, TResponse>> logger,
    IPunchItemRepository punchItemRepository)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var typeName = request.GetGenericTypeName();

        if (request is IIsPunchItemCommand punchItemCommand)
        {
            logger.LogInformation("----- Prefetch PunchItem for {TypeName}", typeName);
            var punchItem = await punchItemRepository.GetOrNullAsync(punchItemCommand.PunchItemGuid, cancellationToken);
            if (punchItem is not null)
            {
                punchItemCommand.PunchItem = punchItem;
            }
            else
            {
                throw new BadRequestException($"Punch item with this guid does not exist! Guid={punchItemCommand.PunchItemGuid}");
            }
        }

        return await next();
    }
}

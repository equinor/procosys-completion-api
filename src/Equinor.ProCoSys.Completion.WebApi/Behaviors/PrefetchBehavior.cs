using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Behaviors;

public class PrefetchBehavior<TRequest, TResponse>(
    IPunchItemRepository punchItemRepository,
    IPunchItemService punchItemService,
    ILogger<PrefetchBehavior<TRequest, TResponse>> logger)
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
                // missing entity should return Not Found - 404 on query requests, but Bad Request - 400 for command requests
                throw new BadRequestException($"Punch item with this guid does not exist! Guid={punchItemCommand.PunchItemGuid}");
            }
        }

        else if (request is IIsPunchItemQuery punchItemQuery)
        {
            logger.LogInformation("----- Prefetch PunchItem for {TypeName}", typeName);
            var punchItemDetailsDto = await punchItemService.GetPunchItemOrNullByPunchItemGuidAsync(punchItemQuery.PunchItemGuid, cancellationToken);
            if (punchItemDetailsDto is not null)
            {
                punchItemQuery.PunchItemDetailsDto = punchItemDetailsDto;
            }
            else
            {
                // missing entity should return Not Found - 404 on query requests, but Bad Request - 400 for command requests
                throw new EntityNotFoundException($"Punch item with this guid does not exist! Guid={punchItemQuery.PunchItemGuid}");
            }
        }

        else if (request is IIsPunchItemRelatedQuery punchItemRelatedQuery)
        {
            logger.LogInformation("----- Prefetch PunchItem for {TypeName}", typeName);
            var projectDetailsDto = await punchItemService.GetProjectOrNullByPunchItemGuidAsync(punchItemRelatedQuery.PunchItemGuid, cancellationToken);
            if (projectDetailsDto is not null)
            {
                punchItemRelatedQuery.ProjectDetailsDto = projectDetailsDto;
            }
            else
            {
                // missing entity should return Not Found - 404 on query requests, but Bad Request - 400 for command requests
                throw new EntityNotFoundException($"Punch item with this guid does not exist! Guid={punchItemRelatedQuery.PunchItemGuid}");
            }
        }

        return await next();
    }
}

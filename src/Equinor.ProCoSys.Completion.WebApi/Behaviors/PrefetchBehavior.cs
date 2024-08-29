using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using Equinor.ProCoSys.Completion.Query.CheckListQueries;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries;
using Equinor.ProCoSys.Completion.Query.PunchItemServices;
using MediatR;
using Microsoft.Extensions.Logging;
using CheckListQueryDetailsDto = Equinor.ProCoSys.Completion.Query.CheckListDetailsDto;
using CheckListCommandDetailsDto = Equinor.ProCoSys.Completion.Command.PunchItemCommands.CheckListDetailsDto;

namespace Equinor.ProCoSys.Completion.WebApi.Behaviors;

public class PrefetchBehavior<TRequest, TResponse>(
    IPunchItemRepository punchItemRepository,
    IPunchItemService punchItemService,
    ICheckListCache checkListCache,
    ILogger<PrefetchBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        logger.LogInformation("----- Prefetch PunchItem for {TypeName}", request.GetGenericTypeName());

        if (request is IIsPunchItemCommand punchItemCommand)
        {
            var punchItem = await punchItemRepository.GetOrNullAsync(punchItemCommand.PunchItemGuid, cancellationToken);
            if (punchItem is not null)
            {
                punchItemCommand.PunchItem = punchItem;
            }
            else
            {
                // missing entity should return Not Found (404) on query requests, but Bad Request (400) for command requests
                throw new BadRequestException($"Punch item with this guid does not exist! Guid={punchItemCommand.PunchItemGuid}");
            }
        }

        else if (request is IIsPunchItemQuery punchItemQuery)
        {
            var punchItemDetailsDto = await punchItemService.GetPunchItemOrNullByPunchItemGuidAsync(punchItemQuery.PunchItemGuid, cancellationToken);
            if (punchItemDetailsDto is not null)
            {
                punchItemQuery.PunchItemDetailsDto = punchItemDetailsDto;
            }
            else
            {
                // missing entity should return Not Found (404) on query requests, but Bad Request (400) for command requests
                throw new EntityNotFoundException($"Punch item with this guid does not exist! Guid={punchItemQuery.PunchItemGuid}");
            }
        }

        else if (request is IIsPunchItemRelatedQuery punchItemRelatedQuery)
        {
            var projectDetailsDto = await punchItemService.GetProjectOrNullByPunchItemGuidAsync(punchItemRelatedQuery.PunchItemGuid, cancellationToken);
            if (projectDetailsDto is not null)
            {
                punchItemRelatedQuery.ProjectDetailsDto = projectDetailsDto;
            }
            else
            {
                // missing entity should return Not Found (404) on query requests, but Bad Request (400) for command requests
                throw new EntityNotFoundException($"Punch item with this guid does not exist! Guid={punchItemRelatedQuery.PunchItemGuid}");
            }
        }


        else if (request is IIsCheckListQuery checkListQuery)
        {
            var proCoSys4CheckList = await checkListCache.GetCheckListAsync(checkListQuery.CheckListGuid, cancellationToken);
            if (proCoSys4CheckList is not null)
            {
                checkListQuery.CheckListDetailsDto =
                    new CheckListQueryDetailsDto(
                        checkListQuery.CheckListGuid,
                        proCoSys4CheckList.FormularType,
                        proCoSys4CheckList.ResponsibleCode,
                        proCoSys4CheckList.TagRegisterCode,
                        proCoSys4CheckList.TagRegisterDescription,
                        proCoSys4CheckList.TagFunctionCode,
                        proCoSys4CheckList.TagFunctionDescription,
                        proCoSys4CheckList.ProjectGuid);
            }
            else
            {
                // missing entity should return Not Found (404) on query requests, but Bad Request (400) for command requests
                throw new EntityNotFoundException($"Check list with this guid does not exist! Guid={checkListQuery.CheckListGuid}");
            }
        }

        // This check must come AFTER prefetching PunchList. GetCheckListGuidForWriteAccessCheck() uses PunchList
        if (request is ICanHaveRestrictionsViaCheckList checkListRequest)
        {
            var checkListGuid = checkListRequest.GetCheckListGuidForWriteAccessCheck();
            var proCoSys4CheckList = await checkListCache.GetCheckListAsync(checkListGuid, cancellationToken);
            if (proCoSys4CheckList is not null)
            {
                checkListRequest.CheckListDetailsDto =
                    new CheckListCommandDetailsDto(
                        checkListGuid,
                        proCoSys4CheckList.ResponsibleCode,
                        proCoSys4CheckList.IsVoided,
                        proCoSys4CheckList.ProjectGuid);
            }
            else
            {
                // missing entity should return Not Found (404) on query requests, but Bad Request (400) for command requests
                throw new BadRequestException($"Check list with this guid does not exist! Guid={checkListGuid}");
            }
        }

        return await next();
    }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.ForeignApi.MainApi.CheckList;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.CacheQueries;

public readonly record struct PrefetchCheckListQuery(Guid CheckListGuid, string Plant) : IRequest;

public sealed class PrefetchCheckListQueryHandler(ICheckListCache checkListCache)
    : IRequestHandler<PrefetchCheckListQuery>
{
    public async Task Handle(PrefetchCheckListQuery request, CancellationToken cancellationToken) =>
        await checkListCache.GetCheckListAsync(request.Plant, request.CheckListGuid, cancellationToken);
}

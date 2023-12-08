using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Query.LabelEntityQueries.GetLabelsForEntity;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.LabelEntities;

[ApiController]
[Route("LabelEntities")]
public class LabelEntitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabelEntitiesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get all non-voided Labels for a given Label entity
    /// </summary>
    /// <param name="entityWithLabelsType">Label entity to get labels for</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [AuthorizeAny(Permissions.LIBRARY_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetLabelsForEntity(
        EntityWithLabelType entityWithLabelsType,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLabelsForEntityQuery(entityWithLabelsType), cancellationToken);
        return this.FromResult(result);
    }
}

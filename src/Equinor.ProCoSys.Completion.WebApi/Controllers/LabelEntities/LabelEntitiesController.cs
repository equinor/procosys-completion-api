using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Query.LabelEntityQueries.GetLabelsForEntityType;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.LabelEntities;

[Authorize]
[ApiController]
[Route("LabelEntities")]
public class LabelEntitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabelEntitiesController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Get all non-voided Labels for a given Label entity type
    /// </summary>
    /// <param name="entityType">Label entity type to get labels for</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetLabelsForEntityType(
        EntityTypeWithLabel entityType,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLabelsForEntityTypeQuery(entityType), cancellationToken);
        return Ok(result);
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;
using Equinor.ProCoSys.Completion.Query.LabelEntityQueries.GetLabelsForEntity;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Labels;

[Authorize]
[ApiController]
[Route("Labels")]
public class LabelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabelsController(IMediator mediator) => _mediator = mediator;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<string>>> GetLabelsForHost(
        EntityWithLabelType entityWithLabelsType,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetLabelsForEntityQuery(entityWithLabelsType), cancellationToken);
        return this.FromResult(result);
    }
}

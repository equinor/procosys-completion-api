using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Command.LabelCommands.CreateLabel;
using Equinor.ProCoSys.Completion.Query.LabelQueries.GetAllLabels;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Labels;

// Todo 108512 Secure with superuser permission
// todo 108269 Create integration tests
[Authorize]
[ApiController]
[Route("Labels")]
public class LabelsController : ControllerBase
{
    private readonly IMediator _mediator;

    public LabelsController(IMediator mediator) => _mediator = mediator;

    /// <summary>
    /// Add a new Label. Need to be unique
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <returns>RowVersion of created Label</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    [HttpPost]
    public async Task<ActionResult<IEnumerable<string>>> CreateLabel(
        CancellationToken cancellationToken,
        [FromBody] CreateLabelDto dto)
    {
        var result = await _mediator.Send(new CreateLabelCommand(dto.Text), cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Get all Labels
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of Labels (or empty list)</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<LabelDto>>> GetLabels(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllLabelsQuery(), cancellationToken);
        return this.FromResult(result);
    }
}

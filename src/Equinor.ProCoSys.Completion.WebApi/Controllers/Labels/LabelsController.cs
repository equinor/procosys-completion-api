using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Completion.Command.LabelCommands.CreateLabel;
using Equinor.ProCoSys.Completion.Command.LabelCommands.UpdateLabelAvailableFor;
using Equinor.ProCoSys.Completion.Query.LabelQueries.GetAllLabels;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Labels;

[AuthorizeAny(Permissions.SUPERUSER, Permissions.APPLICATION_TESTER)]
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
        return Ok(result);
    }

    /// <summary>
    /// Update which Label entities a Label is available for
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="cancellationToken"></param>
    /// <response code="400">Input validation error (error returned in body)</response>
    [HttpPut]
    public async Task<ActionResult> UpdateLabelAvailableFor(
        CancellationToken cancellationToken,
        [FromBody] UpdateLabelAvailableForDto dto)
    {
        await _mediator.Send(new UpdateLabelAvailableForCommand(dto.Text, dto.AvailableForLabels), cancellationToken);
        return Ok();
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
        return Ok(result);
    }
}

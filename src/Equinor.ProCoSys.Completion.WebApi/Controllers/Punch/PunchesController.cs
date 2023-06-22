using MediatR;
using Microsoft.AspNetCore.Mvc;
using ServiceResult.ApiExtensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunch;
using Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunchComment;
using Equinor.ProCoSys.Completion.Command.PunchCommands.CreatePunchLink;
using Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunch;
using Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchAttachment;
using Equinor.ProCoSys.Completion.Command.PunchCommands.DeletePunchLink;
using Equinor.ProCoSys.Completion.Command.PunchCommands.OverwriteExistingPunchAttachment;
using Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunch;
using Equinor.ProCoSys.Completion.Command.PunchCommands.UpdatePunchLink;
using Equinor.ProCoSys.Completion.Command.PunchCommands.UploadNewPunchAttachment;
using Equinor.ProCoSys.Completion.Query.Attachments;
using Equinor.ProCoSys.Completion.Query.Comments;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunch;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchAttachmentDownloadUrl;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchAttachments;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchComments;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchesInProject;
using Equinor.ProCoSys.Completion.Query.PunchQueries.GetPunchLinks;
using Equinor.ProCoSys.Completion.Query.Links;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Punch;

[ApiController]
[Route("Punches")]
public class PunchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public PunchesController(IMediator mediator) => _mediator = mediator;

    #region Punches
    [AuthorizeAny(Permissions.PUNCHLISTITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}")]
    public async Task<ActionResult<PunchDetailsDto>> GetPunchByGuid(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid)
    {
        var result = await _mediator.Send(new GetPunchQuery(guid));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PunchDto>>> GetPunchesInProject(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [Required]
        [FromQuery] Guid projectGuid,
        [FromQuery] bool includeVoided = false)
    {
        var result = await _mediator.Send(new GetPunchesInProjectQuery(projectGuid, includeVoided));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_CREATE, Permissions.APPLICATION_TESTER)]
    [HttpPost]
    public async Task<ActionResult<GuidAndRowVersion>> CreatePunch(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromBody] CreatePunchDto dto)
    {
        var result = await _mediator.Send(new CreatePunchCommand(dto.ItemNo, dto.ProjectGuid));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_WRITE, Permissions.APPLICATION_TESTER)]
    [HttpPut("{guid}")]
    public async Task<ActionResult<string>> UpdatePunch(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromBody] UpdatePunchDto dto)
    {
        var result = await _mediator.Send(
            new UpdatePunchCommand(guid, dto.Description, dto.RowVersion));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_DELETE, Permissions.APPLICATION_TESTER)]
    [HttpDelete("{guid}")]
    public async Task<ActionResult> DeletePunch(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromBody] RowVersionDto dto)
    {
        var result = await _mediator.Send(new DeletePunchCommand(guid, dto.RowVersion));
        return this.FromResult(result);
    }
    #endregion

    #region Links
    [AuthorizeAny(Permissions.PUNCHLISTITEM_ATTACH, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Links")]
    public async Task<ActionResult<GuidAndRowVersion>> CreatePunchLink(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromBody] CreateLinkDto dto)
    {
        var result = await _mediator.Send(new CreatePunchLinkCommand(guid, dto.Title, dto.Url));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Links")]
    public async Task<ActionResult<IEnumerable<LinkDto>>> GetPunchLinks(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid)
    {
        var result = await _mediator.Send(new GetPunchLinksQuery(guid));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_WRITE, Permissions.APPLICATION_TESTER)]
    [HttpPut("{guid}/Links/{linkGuid}")]
    public async Task<ActionResult<string>> UpdatePunchLink(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromRoute] Guid linkGuid,
        [FromBody] UpdateLinkDto dto)
    {
        var result = await _mediator.Send(new UpdatePunchLinkCommand(guid, linkGuid, dto.Title, dto.Url, dto.RowVersion));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_DELETE, Permissions.APPLICATION_TESTER)]
    [HttpDelete("{guid}/Links/{linkGuid}")]
    public async Task<ActionResult> DeleteLink(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromRoute] Guid linkGuid,
        [FromBody] RowVersionDto dto)
    {
        var result = await _mediator.Send(new DeletePunchLinkCommand(guid, linkGuid, dto.RowVersion));
        return this.FromResult(result);
    }
    #endregion

    #region Comments
    [AuthorizeAny(Permissions.PUNCHLISTITEM_WRITE, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Comments")]
    public async Task<ActionResult<GuidAndRowVersion>> CreatePunchComment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromBody] CreateCommentDto dto)
    {
        var result = await _mediator.Send(new CreatePunchCommentCommand(guid, dto.Text));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetPunchComments(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid)
    {
        var result = await _mediator.Send(new GetPunchCommentsQuery(guid));
        return this.FromResult(result);
    }
    #endregion

    #region Attachments
    [AuthorizeAny(Permissions.PUNCHLISTITEM_ATTACH, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Attachments")]
    public async Task<ActionResult<GuidAndRowVersion>> UploadPunchAttachment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromForm] UploadNewAttachmentDto dto)
    {
        await using var stream = dto.File.OpenReadStream();

        var result = await _mediator.Send(new UploadNewPunchAttachmentCommand(
            guid,
            dto.File.FileName,
            stream));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_ATTACH, Permissions.APPLICATION_TESTER)]
    [HttpPut("{guid}/Attachments")]
    public async Task<ActionResult<string>> OverwriteExistingPunchAttachment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromForm] OverwriteAttachmentDto dto)
    {
        await using var stream = dto.File.OpenReadStream();

        var result = await _mediator.Send(new OverwriteExistingPunchAttachmentCommand(
            guid, 
            dto.File.FileName,
            dto.RowVersion,
            stream));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Attachments")]
    public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetPunchAttachments(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid)
    {
        var result = await _mediator.Send(new GetPunchAttachmentsQuery(guid));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_DELETE, Permissions.APPLICATION_TESTER)]
    [HttpDelete("{guid}/Attachments/{attachmentGuid}")]
    public async Task<ActionResult> DeleteAttachment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromRoute] Guid attachmentGuid,
        [FromBody] RowVersionDto dto)
    {
        var result = await _mediator.Send(new DeletePunchAttachmentCommand(guid, attachmentGuid, dto.RowVersion));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHLISTITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Attachments/{attachmentGuid}")]
    public async Task<ActionResult<string>> GetPunchAttachmentDownloadUrl(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromRoute] Guid attachmentGuid)
    {
        var result = await _mediator.Send(new GetPunchAttachmentDownloadUrlQuery(guid, attachmentGuid));

        if (result.ResultType != ResultType.Ok)
        {
            return this.FromResult(result);
        }

        return Ok(result.Data.ToString());
    }
    #endregion
}

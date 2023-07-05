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
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemLink;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.OverwriteExistingPunchItemAttachment;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemLink;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;
using Equinor.ProCoSys.Completion.Query.Attachments;
using Equinor.ProCoSys.Completion.Query.Comments;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachments;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemComments;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemLinks;
using Equinor.ProCoSys.Completion.Query.Links;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using ServiceResult;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Comments;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Links;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

[ApiController]
[Route("PunchItems")]
public class PunchItemsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PunchItemsController(IMediator mediator) => _mediator = mediator;

    #region PunchItems
    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}")]
    public async Task<ActionResult<PunchItemDetailsDto>> GetPunchItemByGuid(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid)
    {
        var result = await _mediator.Send(new GetPunchItemQuery(guid));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PunchItemDto>>> GetPunchItemsInProject(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [Required]
        [FromQuery] Guid projectGuid)
    {
        var result = await _mediator.Send(new GetPunchItemsInProjectQuery(projectGuid));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_CREATE, Permissions.APPLICATION_TESTER)]
    [HttpPost]
    public async Task<ActionResult<GuidAndRowVersion>> CreatePunchItem(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromBody] CreatePunchItemDto dto)
    {
        
        var result = await _mediator.Send(new CreatePunchItemCommand(dto.ItemNo, dto.ProjectGuid));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_WRITE, Permissions.APPLICATION_TESTER)]
    [HttpPut("{guid}")]
    public async Task<ActionResult<string>> UpdatePunchItem(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromBody] UpdatePunchItemDto dto)
    {
        var result = await _mediator.Send(
            new UpdatePunchItemCommand(guid, dto.Description, dto.RowVersion));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_DELETE, Permissions.APPLICATION_TESTER)]
    [HttpDelete("{guid}")]
    public async Task<ActionResult> DeletePunchItem(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromBody] RowVersionDto dto)
    {
        var result = await _mediator.Send(new DeletePunchItemCommand(guid, dto.RowVersion));
        return this.FromResult(result);
    }
    #endregion

    #region Links
    [AuthorizeAny(Permissions.PUNCHITEM_ATTACH, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Links")]
    public async Task<ActionResult<GuidAndRowVersion>> CreatePunchItemLink(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromBody] CreateLinkDto dto)
    {
        var result = await _mediator.Send(new CreatePunchItemLinkCommand(guid, dto.Title, dto.Url));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Links")]
    public async Task<ActionResult<IEnumerable<LinkDto>>> GetPunchItemLinks(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid)
    {
        var result = await _mediator.Send(new GetPunchItemLinksQuery(guid));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_WRITE, Permissions.APPLICATION_TESTER)]
    [HttpPut("{guid}/Links/{linkGuid}")]
    public async Task<ActionResult<string>> UpdatePunchItemLink(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromRoute] Guid linkGuid,
        [FromBody] UpdateLinkDto dto)
    {
        var result = await _mediator.Send(new UpdatePunchItemLinkCommand(guid, linkGuid, dto.Title, dto.Url, dto.RowVersion));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_DELETE, Permissions.APPLICATION_TESTER)]
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
        var result = await _mediator.Send(new DeletePunchItemLinkCommand(guid, linkGuid, dto.RowVersion));
        return this.FromResult(result);
    }
    #endregion

    #region Comments
    [AuthorizeAny(Permissions.PUNCHITEM_WRITE, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Comments")]
    public async Task<ActionResult<GuidAndRowVersion>> CreatePunchItemComment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromBody] CreateCommentDto dto)
    {
        var result = await _mediator.Send(new CreatePunchItemCommentCommand(guid, dto.Text));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetPunchItemComments(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid)
    {
        var result = await _mediator.Send(new GetPunchItemCommentsQuery(guid));
        return this.FromResult(result);
    }
    #endregion

    #region Attachments
    [AuthorizeAny(Permissions.PUNCHITEM_ATTACH, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Attachments")]
    public async Task<ActionResult<GuidAndRowVersion>> UploadPunchItemAttachment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromForm] UploadNewAttachmentDto dto)
    {
        await using var stream = dto.File.OpenReadStream();

        var result = await _mediator.Send(new UploadNewPunchItemAttachmentCommand(
            guid,
            dto.File.FileName,
            stream));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_ATTACH, Permissions.APPLICATION_TESTER)]
    [HttpPut("{guid}/Attachments")]
    public async Task<ActionResult<string>> OverwriteExistingPunchItemAttachment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromForm] OverwriteAttachmentDto dto)
    {
        await using var stream = dto.File.OpenReadStream();

        var result = await _mediator.Send(new OverwriteExistingPunchItemAttachmentCommand(
            guid, 
            dto.File.FileName,
            dto.RowVersion,
            stream));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Attachments")]
    public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetPunchItemAttachments(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid)
    {
        var result = await _mediator.Send(new GetPunchItemAttachmentsQuery(guid));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_DELETE, Permissions.APPLICATION_TESTER)]
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
        var result = await _mediator.Send(new DeletePunchItemAttachmentCommand(guid, attachmentGuid, dto.RowVersion));
        return this.FromResult(result);
    }

    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Attachments/{attachmentGuid}")]
    public async Task<ActionResult<string>> GetPunchItemAttachmentDownloadUrl(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        [FromRoute] Guid guid,
        [FromRoute] Guid attachmentGuid)
    {
        var result = await _mediator.Send(new GetPunchItemAttachmentDownloadUrlQuery(guid, attachmentGuid));

        if (result.ResultType != ResultType.Ok)
        {
            return this.FromResult(result);
        }

        return Ok(result.Data.ToString());
    }
    #endregion
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Command;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.ClearPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemComment;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.CreatePunchItemLink;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemAttachment;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.DeletePunchItemLink;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.OverwriteExistingPunchItemAttachment;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.RejectPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnclearPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UnverifyPunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItem;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemAttachment;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemCategory;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UpdatePunchItemLink;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.UploadNewPunchItemAttachment;
using Equinor.ProCoSys.Completion.Command.PunchItemCommands.VerifyPunchItem;
using Equinor.ProCoSys.Completion.Query.Attachments;
using Equinor.ProCoSys.Completion.Query.Comments;
using Equinor.ProCoSys.Completion.Query.Links;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItem;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachmentDownloadUrl;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemAttachments;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemComments;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemLinks;
using Equinor.ProCoSys.Completion.Query.PunchItemQueries.GetPunchItemsInProject;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Attachments;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Comments;
using Equinor.ProCoSys.Completion.WebApi.Controllers.Links;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Equinor.ProCoSys.Completion.WebApi.Swagger;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ServiceResult;
using ServiceResult.ApiExtensions;
using Swashbuckle.AspNetCore.Filters;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

/// <summary>
/// PunchItem endpoints
/// </summary>
[ApiController]
[Route("PunchItems")]
public class PunchItemsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PunchItemsController> _logger;

    public PunchItemsController(IMediator mediator, ILogger<PunchItemsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    #region PunchItems

    /// <summary>
    /// Get a PunchItem by its Guid
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <returns>Found PunchItem</returns>
    /// <response code="404">PunchItem not found</response>
    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}")]
    public async Task<ActionResult<PunchItemDetailsDto>> GetPunchItemByGuid(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid)
    {
        var result = await _mediator.Send(new GetPunchItemQuery(guid), cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Get all PunchItems in given project (no filtering available yet)
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="projectGuid">Guid of project where to get PunchItems</param>
    /// <returns>List of PunchItems (or empty list)</returns>
    /// <response code="404">Project not found</response>
    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PunchItemDto>>> GetPunchItemsInProject(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [Required] [FromQuery] Guid projectGuid)
    {
        var result = await _mediator.Send(new GetPunchItemsInProjectQuery(projectGuid), cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Create new PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="dto"></param>
    /// <returns>Guid and RowVersion of created PunchItem</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    [AuthorizeAny(Permissions.PUNCHITEM_CREATE, Permissions.APPLICATION_TESTER)]
    [HttpPost]
    public async Task<ActionResult<GuidAndRowVersion>> CreatePunchItem(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromBody] CreatePunchItemDto dto)
    {
        var result = await _mediator.Send(new CreatePunchItemCommand(
                dto.Category,
                dto.Description,
                dto.ProjectGuid,
                dto.CheckListGuid,
                dto.RaisedByOrgGuid,
                dto.ClearingByOrgGuid,
                dto.ActionByPersonOid,
                dto.DueTimeUtc,
                dto.PriorityGuid,
                dto.SortingGuid,
                dto.TypeGuid,
                dto.Estimate,
                dto.OriginalWorkOrderGuid,
                dto.WorkOrderGuid,
                dto.SWCRGuid,
                dto.DocumentGuid,
                dto.ExternalItemNo,
                dto.MaterialRequired,
                dto.MaterialETAUtc,
                dto.MaterialExternalNo),
            cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Update (patch) a PunchItem
    /// </summary>
    /// <description>test</description>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto"></param>
    /// <returns>New RowVersion of PunchItem. If no changes done, same RowVersion as previous returned</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    /// <response code="404">Not found</response>
    [AuthorizeAny(Permissions.PUNCHITEM_WRITE, Permissions.APPLICATION_TESTER)]
    [HttpPatch("{guid}")]
    [SwaggerPatchDocumentation(typeof(PatchPunchItemDto))]
    [SwaggerRequestExample(typeof(PatchPunchItemDto), typeof(PatchPunchItemDtoExample))]
    public async Task<ActionResult<string>> UpdatePunchItem(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromBody] PatchPunchItemDto dto)
    {
        var result = await _mediator.Send(
            new UpdatePunchItemCommand(guid, dto.PatchDocument, dto.RowVersion),
            cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Change category on PunchItem (PA->PB or PB->PA)
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto">dto</param>
    /// <returns>New RowVersion of PunchItem</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    /// <response code="404">Not found</response>
    /// <remarks>Will give validation error if set same category as PunchItem already have</remarks>
    [AuthorizeAny(Permissions.PUNCHITEM_WRITE, Permissions.APPLICATION_TESTER)]
    [HttpPatch("{guid}/UpdateCategory")]
    public async Task<ActionResult<string>> UpdatePunchItemCategory(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromBody] UpdatePunchItemCategoryDto dto)
    {
        var result = await _mediator.Send(
            new UpdatePunchItemCategoryCommand(guid, dto.Category, dto.RowVersion),
            cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Clear new or rejected PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto"></param>
    /// <returns>New RowVersion of PunchItem</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    [AuthorizeAny(Permissions.PUNCHITEM_CLEAR, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Clear")]
    public async Task<ActionResult<string>> ClearPunchItem(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromBody] RowVersionDto dto)
    {
        var result = await _mediator.Send(
            new ClearPunchItemCommand(guid, dto.RowVersion), cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Unclear cleared PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto"></param>
    /// <returns>New RowVersion of PunchItem</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    [AuthorizeAny(Permissions.PUNCHITEM_CLEAR, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Unclear")]
    public async Task<ActionResult<string>> UnclearPunchItem(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromBody] RowVersionDto dto)
    {
        var result = await _mediator.Send(
            new UnclearPunchItemCommand(guid, dto.RowVersion), cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Reject cleared PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto"></param>
    /// <returns>New RowVersion of PunchItem</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    [AuthorizeAny(Permissions.PUNCHITEM_CLEAR, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Reject")]
    public async Task<ActionResult<string>> RejectPunchItem(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromBody] RejectPunchItemDto dto)
    {
        var result = await _mediator.Send(
            new RejectPunchItemCommand(guid, dto.Comment, dto.Mentions, dto.RowVersion), cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Verify cleared PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto"></param>
    /// <returns>New RowVersion of PunchItem</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    [AuthorizeAny(Permissions.PUNCHITEM_VERIFY, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Verify")]
    public async Task<ActionResult<string>> VerifyPunchItem(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromBody] RowVersionDto dto)
    {
        var result = await _mediator.Send(
            new VerifyPunchItemCommand(guid, dto.RowVersion), cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Unverify verified PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto"></param>
    /// <returns>New RowVersion of PunchItem</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    [AuthorizeAny(Permissions.PUNCHITEM_VERIFY, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Unverify")]
    public async Task<ActionResult<string>> UnverifyPunchItem(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromBody] RowVersionDto dto)
    {
        var result = await _mediator.Send(
            new UnverifyPunchItemCommand(guid, dto.RowVersion), cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Delete PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto"></param>
    [AuthorizeAny(Permissions.PUNCHITEM_DELETE, Permissions.APPLICATION_TESTER)]
    [HttpDelete("{guid}")]
    public async Task<ActionResult> DeletePunchItem(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromBody] RowVersionDto dto)
    {
        var result = await _mediator.Send(new DeletePunchItemCommand(guid, dto.RowVersion), cancellationToken);
        return this.FromResult(result);
    }

    #endregion

    #region Links

    /// <summary>
    /// Add link to PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto"></param>
    /// <returns>Guid and RowVersion of created link</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    [AuthorizeAny(Permissions.PUNCHITEM_ATTACH, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Links")]
    public async Task<ActionResult<GuidAndRowVersion>> CreatePunchItemLink(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromBody] CreateLinkDto dto)
    {
        var result = await _mediator.Send(
            new CreatePunchItemLinkCommand(guid, dto.Title, dto.Url),
            cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Get all links on a PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <returns>List of links (or empty list)</returns>
    /// <response code="404">PunchItem not found</response>
    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Links")]
    public async Task<ActionResult<IEnumerable<LinkDto>>> GetPunchItemLinks(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid)
    {
        var result = await _mediator.Send(new GetPunchItemLinksQuery(guid), cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Update a link on a PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="linkGuid">Guid on link</param>
    /// <param name="dto"></param>
    /// <returns>New RowVersion of link</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    [AuthorizeAny(Permissions.PUNCHITEM_WRITE, Permissions.APPLICATION_TESTER)]
    [HttpPut("{guid}/Links/{linkGuid}")]
    public async Task<ActionResult<string>> UpdatePunchItemLink(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromRoute] Guid linkGuid,
        [FromBody] UpdateLinkDto dto)
    {
        var result = await _mediator.Send(
            new UpdatePunchItemLinkCommand(guid, linkGuid, dto.Title, dto.Url, dto.RowVersion),
            cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Delete a link from a PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="linkGuid">Guid on link</param>
    /// <param name="dto"></param>
    [AuthorizeAny(Permissions.PUNCHITEM_DELETE, Permissions.APPLICATION_TESTER)]
    [HttpDelete("{guid}/Links/{linkGuid}")]
    public async Task<ActionResult> DeleteLink(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromRoute] Guid linkGuid,
        [FromBody] RowVersionDto dto)
    {
        var result = await _mediator.Send(
            new DeletePunchItemLinkCommand(guid, linkGuid, dto.RowVersion),
            cancellationToken);
        return this.FromResult(result);
    }

    #endregion

    #region Comments

    /// <summary>
    /// Add a comment to a PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto"></param>
    /// <returns>Guid and RowVersion of created comment</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    [AuthorizeAny(Permissions.PUNCHITEM_WRITE, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Comments")]
    public async Task<ActionResult<GuidAndRowVersion>> CreatePunchItemComment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromBody] CreateCommentDto dto)
    {
        var result = await _mediator.Send(
            new CreatePunchItemCommentCommand(guid, dto.Text, dto.Labels, dto.Mentions), cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Get all comments on a PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <returns>List of comments (or empty list)</returns>
    /// <response code="404">PunchItem not found</response>
    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Comments")]
    public async Task<ActionResult<IEnumerable<CommentDto>>> GetPunchItemComments(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid)
    {
        var result = await _mediator.Send(new GetPunchItemCommentsQuery(guid), cancellationToken);
        return this.FromResult(result);
    }

    #endregion

    #region Attachments

    /// <summary>
    /// Add (upload) a new attachment/picture to a PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto"></param>
    /// <returns>Guid and RowVersion of created attachment/picture</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    /// <remarks>Will give validation error if uploading same attachment with same filename as an existing attachment. Use PUT endpoint if overwrite is intended</remarks>
    [AuthorizeAny(Permissions.PUNCHITEM_ATTACH, Permissions.APPLICATION_TESTER)]
    [HttpPost("{guid}/Attachments")]
    public async Task<ActionResult<GuidAndRowVersion>> UploadPunchItemAttachment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromForm] UploadNewAttachmentDto dto)
    {
        await using var stream = dto.File.OpenReadStream();

        var result = await _mediator.Send(new UploadNewPunchItemAttachmentCommand(
            guid,
            dto.File.FileName,
            stream,
            dto.File.ContentType), 
            cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Add (upload) and overwrite an existing attachment/picture on a PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="dto"></param>
    /// <returns>New RowVersion of attachment/picture</returns>
    /// <response code="400">Input validation error (error returned in body)</response>
    /// <remarks>Will give validation error if attachment with same filename don't exist. Use POST endpoint if uploading new attachment is intended</remarks>
    [AuthorizeAny(Permissions.PUNCHITEM_ATTACH, Permissions.APPLICATION_TESTER)]
    [HttpPut("{guid}/Attachments")]
    public async Task<ActionResult<string>> OverwriteExistingPunchItemAttachment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromForm] OverwriteAttachmentDto dto)
    {
        await using var stream = dto.File.OpenReadStream();

        var result = await _mediator.Send(new OverwriteExistingPunchItemAttachmentCommand(
            guid, 
            dto.File.FileName,
            dto.RowVersion,
            stream,
            dto.File.ContentType), 
            cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Get all attachments/pictures on a PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <returns>List of attachments/pictures (or empty list)</returns>
    /// <response code="404">PunchItem not found</response>
    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Attachments")]
    public async Task<ActionResult<IEnumerable<AttachmentDto>>> GetPunchItemAttachments(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid)
    {
        var ipAddress = GetClientIpAddress();

        var result = await _mediator.Send(new GetPunchItemAttachmentsQuery(guid, ipAddress, null), cancellationToken);
        return this.FromResult(result);
    }

    private string? GetIpAddressFromHeaders()
    {
        var proCoSysForwardHeader = Request.Headers["X-Forwarded-For-ProCoSys"].FirstOrDefault();
        if (!string.IsNullOrEmpty(proCoSysForwardHeader))
        {
            return proCoSysForwardHeader;
        }

        var forwardedForHeader = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedForHeader))
        {
            return forwardedForHeader;
        }

        var realIpHeader = Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIpHeader))
        {
            return realIpHeader;
        }

        _logger.LogInformation("No headers found, using connection: {IpAddress}",
            Request.HttpContext.Connection.RemoteIpAddress?.ToString());
        return Request.HttpContext.Connection.RemoteIpAddress?.ToString();
    }
    
    private string? GetClientIpAddress()
    {
        var ipAddress = GetIpAddressFromHeaders();
        
        if (string.IsNullOrEmpty(ipAddress))
        {
            _logger.LogWarning("No IP address found");
            return null;
        }
        
        if (ipAddress.Contains(','))
        {
            var ipAddresses = ipAddress.Split(",");
            ipAddress = ipAddresses[0].Trim();
        }

        if (ipAddress.Contains(':'))
        {
            var ipAddresses = ipAddress.Split(":");
            ipAddress = ipAddresses[0].Trim();
        }
        
        _logger.LogInformation("Using address: {IpAddress}", ipAddress);
        return ipAddress;
    }

    /// <summary>
    /// Delete an attachment/picture from a PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="attachmentGuid">Guid on attachment/picture</param>
    /// <param name="dto"></param>
    [AuthorizeAny(Permissions.PUNCHITEM_DETACH, Permissions.APPLICATION_TESTER)]
    [HttpDelete("{guid}/Attachments/{attachmentGuid}")]
    public async Task<ActionResult> DeleteAttachment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromRoute] Guid attachmentGuid,
        [FromBody] RowVersionDto dto)
    {
        var result = await _mediator.Send(
            new DeletePunchItemAttachmentCommand(guid, attachmentGuid, dto.RowVersion),
            cancellationToken);
        return this.FromResult(result);
    }

    /// <summary>
    /// Get the download url for an attachment/picture on a PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="attachmentGuid">Guid on attachment/picture</param>
    /// <returns>Download url</returns>
    /// <response code="404">PunchItem or attachment/picture not found</response>
    /// <remarks>The url is valid for a short period (some minutes). The period is configured in app settings, key BlobClockSkewMinutes</remarks>
    [AuthorizeAny(Permissions.PUNCHITEM_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("{guid}/Attachments/{attachmentGuid}")]
    public async Task<ActionResult<string>> GetPunchItemAttachmentDownloadUrl(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromRoute] Guid attachmentGuid)
    {
        var result = await _mediator.Send(
            new GetPunchItemAttachmentDownloadUrlQuery(guid, attachmentGuid),
            cancellationToken);

        if (result.ResultType != ResultType.Ok)
        {
            return this.FromResult(result);
        }

        return Ok(result.Data.ToString());
    }

    /// <summary>
    /// Update description and labels on an attachment/picture on a PunchItem
    /// </summary>
    /// <param name="plant">ID of plant in PCS$PLANT format</param>
    /// <param name="cancellationToken"></param>
    /// <param name="guid">Guid on PunchItem</param>
    /// <param name="attachmentGuid">Guid on attachment/picture</param>
    /// <param name="dto"></param>
    /// <returns>New RowVersion of attachment/picture</returns>
    /// <response code="404">PunchItem or attachment/picture not found</response>
    [AuthorizeAny(Permissions.PUNCHITEM_ATTACH, Permissions.APPLICATION_TESTER)]
    [HttpPut("{guid}/Attachments/{attachmentGuid}")]
    public async Task<ActionResult<string>> UpdatePunchItemAttachment(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant,
        CancellationToken cancellationToken,
        [FromRoute] Guid guid,
        [FromRoute] Guid attachmentGuid,
        [FromBody] UpdateAttachmentDto dto)
    {
        var result = await _mediator.Send(
            new UpdatePunchItemAttachmentCommand(guid, attachmentGuid, dto.Description, dto.Labels, dto.RowVersion),
            cancellationToken);
        return this.FromResult(result);
    }

    #endregion
}

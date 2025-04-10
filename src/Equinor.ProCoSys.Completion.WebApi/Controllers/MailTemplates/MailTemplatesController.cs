using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Completion.Query.MailTemplateQueries.GetAllMailTemplates;
using Equinor.ProCoSys.Completion.TieImport.CommonLib;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Equinor.TI.CommonLibrary.Mapper.Core;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.MailTemplates;

[AuthorizeAny(Permissions.SUPERUSER, Permissions.APPLICATION_TESTER)]
[ApiController]
[Route("MailTemplates")]
public class MailTemplatesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IEmailService _emailService;
    //private readonly ISchemaSource _schemaSource;
    private readonly IServiceProvider _serviceProvider;

    public MailTemplatesController(IMediator mediator, IEmailService emailService, IServiceProvider serviceProvide)
    {
        _mediator = mediator;
        _emailService = emailService;
        _serviceProvider = serviceProvide;
        //        _schemaSource = serviceProvide.GetRequiredKeyedService<ISchemaSource>("CommonLibApiSource");
    }

    /// <summary>
    /// Get all mail templates
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns>List of mail templates (or empty list)</returns>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MailTemplateDto>>> GetMailTemplates(CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetAllMailTemplatesQuery(), cancellationToken);
        return Ok(result);
    }

    [HttpGet("Test_CommonLibApiSource-Delete-After-Test")]
    public void TestCommonLibApiSource(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)]
        [Required]
        [StringLength(PlantEntityBase.PlantLengthMax, MinimumLength = PlantEntityBase.PlantLengthMin)]
        string plant
        , CancellationToken cancellationToken)
    {
        var schemaSource = _serviceProvider.GetRequiredKeyedService<ISchemaSource>("CommonLibApiSource");
        var options = _serviceProvider.GetRequiredService<IOptions<CommonLibOptions>>();
        var schema = schemaSource.Get(options.Value.SchemaFrom[0], options.Value.SchemaTo);

        Console.WriteLine(schema);
    }
}

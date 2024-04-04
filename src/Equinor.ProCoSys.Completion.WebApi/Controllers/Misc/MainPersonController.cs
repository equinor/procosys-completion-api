using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Auth.Person;
using Equinor.ProCoSys.Completion.WebApi.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Misc;

[ApiController]
[Route("MainPersons")]
public class MainPersonController : ControllerBase
{
    private readonly IPersonCache _personCache;

    public MainPersonController(
        IPersonCache personCache) => _personCache = personCache;

    [AuthorizeAny(Permissions.USER_READ, Permissions.APPLICATION_TESTER)]
    [HttpGet("All")]
    public async Task<IList<ProCoSysPerson>> GetPersonsFromMainApi(
        [FromHeader(Name = CurrentPlantMiddleware.PlantHeader)] [Required]
        string plant,
        CancellationToken cancellationToken) => await _personCache.GetAllPersonsAsync(plant, cancellationToken);
}

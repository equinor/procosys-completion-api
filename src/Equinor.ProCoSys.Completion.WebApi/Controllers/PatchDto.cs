using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.JsonPatch;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public abstract class PatchDto
{
    [Required]
    public JsonPatchDocument PatchDocument { get; set; } = null!;
}

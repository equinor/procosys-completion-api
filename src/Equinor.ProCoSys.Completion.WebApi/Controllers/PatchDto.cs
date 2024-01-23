using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.JsonPatch;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public abstract class PatchDto<T> where T : class
{
    [Required]
    public JsonPatchDocument<T> PatchDocument { get; set; } = null!;
}

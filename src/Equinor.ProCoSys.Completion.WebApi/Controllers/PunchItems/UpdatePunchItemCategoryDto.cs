using System.ComponentModel.DataAnnotations;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.PunchItems;

public record UpdatePunchItemCategoryDto(
    [Required]
    Category Category,
    [Required]
    string RowVersion);

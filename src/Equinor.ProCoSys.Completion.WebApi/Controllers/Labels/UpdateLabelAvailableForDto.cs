using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Labels;

public record UpdateLabelAvailableForDto(
    [Required]
    string Text,
    [Required]
    List<EntityTypeWithLabel> AvailableForLabels);

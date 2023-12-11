using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.Query.LabelQueries.GetAllLabels;

public record LabelDto(string Text, bool IsVoided, List<string> EntitiesWithLabel);

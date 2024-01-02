using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.Labels;

public record LabelDto(string Text, bool IsVoided, List<string> AvailableFor);

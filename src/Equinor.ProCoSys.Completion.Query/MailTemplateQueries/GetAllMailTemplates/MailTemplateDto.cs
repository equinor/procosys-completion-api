using Equinor.ProCoSys.Common.Misc;

namespace Equinor.ProCoSys.Completion.Query.MailTemplateQueries.GetAllMailTemplates;

public record MailTemplateDto(string Code, string Subject, string Body, bool IsVoided, string? Plant)
{
    public bool IsGlobal => Plant.IsEmpty();
}

#nullable enable

namespace Equinor.ProCoSys.Completion.WebApi.IntegrationTests.MailTemplates;

public record MailTemplateDto(string Code, string Subject, string Body, bool IsVoided, string? Plant, bool IsGlobal);

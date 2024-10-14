using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.MailEvents;

public record SendEmailEvent(List<string> To, string Subject, string Body);

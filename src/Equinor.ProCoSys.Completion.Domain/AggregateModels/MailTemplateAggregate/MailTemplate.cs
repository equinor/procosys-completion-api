using System;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;

public class MailTemplate : EntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IVoidable
{
    public const int CodeLengthMax = 64;
    public const int SubjectLengthMax = 512;
    public const int BodyLengthMax = 2048;
    public const int PlantLengthMax = 255;

#pragma warning disable CS8618
    protected MailTemplate()
#pragma warning restore CS8618
    {
    }

    public MailTemplate(string code, string subject, string body)
    {
        Code = code;
        Subject = subject;
        Body = body;
    }

    // private setters needed for Entity Framework
    public string Code { get; private set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Person CreatedBy { get; private set; } = null!;
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Person? ModifiedBy { get; private set; }
    public bool IsVoided { get; set; }
    // Plant in MailTemplate is not required. Hence: Do not inherit PlantEntityBase
    // A MailTemplate without Plant set is GLOBAL (i.e. valid for all Plants)
    // A MailTemplate with Plant set is valid for that particular Plant only
    public string? Plant { get; set; }

    public void SetCreated(Person createdBy)
    {
        CreatedAtUtc = TimeService.UtcNow;
        CreatedById = createdBy.Id;
        CreatedBy = createdBy;
    }

    public void SetModified(Person modifiedBy)
    {
        ModifiedAtUtc = TimeService.UtcNow;
        if (modifiedBy is null)
        {
            throw new ArgumentNullException(nameof(modifiedBy));
        }
        ModifiedById = modifiedBy.Id;
        ModifiedBy = modifiedBy;
    }
}

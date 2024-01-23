using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;

public class Label : EntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IVoidable
{
    private readonly List<LabelEntity> _availableFor = new();
    public const int TextLengthMax = 60;

    public Label(string text) => Text = text;

    // private setters needed for Entity Framework
    public string Text { get; private set; }
    public bool IsVoided { get; set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Person CreatedBy { get; private set; } = null!;
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Person? ModifiedBy { get; private set; }
    public ICollection<LabelEntity> AvailableFor => _availableFor;

    public void MakeLabelAvailableFor(LabelEntity labelEntity) => _availableFor.Add(labelEntity);

    public void RemoveLabelAvailableFor(LabelEntity labelEntity) => _availableFor.Remove(labelEntity);

    public void SetCreated(Person createdBy)
    {
        CreatedAtUtc = TimeService.UtcNow;
        CreatedBy = createdBy;
        CreatedById = createdBy.Id;
    }

    public void SetModified(Person modifiedBy)
    {
        ModifiedAtUtc = TimeService.UtcNow;
        ModifiedBy = modifiedBy;
        ModifiedById = modifiedBy.Id;
    }
}

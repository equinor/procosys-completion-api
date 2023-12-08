using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelEntityAggregate;

public class LabelEntity : EntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable
{
    private readonly List<Label> _labels = new();

    public LabelEntity(EntityWithLabelType entityWithLabel) => EntityWithLabel = entityWithLabel;

    // private setters needed for Entity Framework
    public EntityWithLabelType EntityWithLabel { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Person CreatedBy { get; private set; } = null!;
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Person? ModifiedBy { get; private set; }
    public ICollection<Label> Labels => _labels;

    public void AddLabel(Label label) => _labels.Add(label);

    public void RemoveLabel(Label label) => _labels.Remove(label);

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

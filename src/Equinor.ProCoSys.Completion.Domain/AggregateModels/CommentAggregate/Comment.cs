using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.CommentAggregate;

public class Comment : EntityBase, IAggregateRoot, ICreationAuditable, IBelongToParent, IHaveGuid, IHaveLabels
{
    public const int ParentTypeLengthMax = 256;
    public const int TextLengthMax = 4000;

    private readonly List<Label> _labels = new();

    public Comment(string parentType, Guid parentGuid, string text)
    {
        ParentType = parentType;
        ParentGuid = parentGuid;
        Text = text;
        Guid = MassTransit.NewId.NextGuid();
    }

    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();
    public IOrderedEnumerable<Label> GetOrderedNonVoidedLabels()
        => _labels.Where(l => !l.IsVoided).OrderBy(l => l.Text);

    // private setters needed for Entity Framework
    public string ParentType { get; private set; }
    public string Text { get; private set; }
    public Guid ParentGuid { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Person CreatedBy { get; private set; } = null!;
    public Guid Guid { get; private set; }

    public void SetCreated(Person createdBy)
    {
        CreatedAtUtc = TimeService.UtcNow;
        CreatedById = createdBy.Id;
        CreatedBy = createdBy;
    }

    public void UpdateLabels(IList<Label> labels)
    {
        RemoveRemovedLabels(labels);
        AddNewLabels(labels);
    }

    private void AddNewLabels(IList<Label> labels)
    {
        foreach (var label in labels)
        {
            if (!_labels.Contains(label))
            {
                _labels.Add(label);
            }
        }
    }

    private void RemoveRemovedLabels(IList<Label> labels)
    {
        for (var i = _labels.Count - 1; i >= 0; i--)
        {
            var label = _labels[i];
            if (!labels.Contains(label))
            {
                _labels.RemoveAt(i);
            }
        }
    }
}

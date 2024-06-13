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
    private readonly List<Person> _mentions = new();

    public Comment(string parentType, Guid parentGuid, string text)
    {
        ParentType = parentType;
        ParentGuid = parentGuid;
        Text = text;
        Guid = MassTransit.NewId.NextGuid();
    }
    public Comment(string parentType, Guid parentGuid, string text, Guid guid)
    {
        ParentType = parentType;
        ParentGuid = parentGuid;
        Text = text;
        Guid = guid;
    }

    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();
    public IOrderedEnumerable<Label> GetOrderedNonVoidedLabels()
        => _labels.Where(l => !l.IsVoided).OrderBy(l => l.Text);

    public IReadOnlyCollection<Person> Mentions => _mentions.AsReadOnly();
    public IOrderedEnumerable<Person> GetOrderedMentions()
        => _mentions.OrderBy(p => p.FirstName).ThenBy(p => p.LastName);

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

    public void SetMentions(IList<Person> mentions)
    {
        _mentions.Clear();
        _mentions.AddRange(mentions);
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

    public void SetSyncProperties(DateTime busEventCreatedAt, 
        Person? createdBy
        )
    {
        if(createdBy != null)
        {
            CreatedBy = createdBy;
        }
        
        CreatedAtUtc = busEventCreatedAt;
        
       
    }
}

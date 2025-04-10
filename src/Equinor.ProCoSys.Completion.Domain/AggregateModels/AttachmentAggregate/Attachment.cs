﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Equinor.ProCoSys.Common;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;

public class Attachment : EntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IBelongToParent, IHaveGuid, IHaveLabels
{
    public const int ParentTypeLengthMax = 256;
    public const int FileNameLengthMax = 255;
    public const int DescriptionLengthMax = 255;
    public const int BlobPathLengthMax = 1024;
    public const int ProCoSys4LastUpdatedByUserLengthMax = 120;

    private readonly List<Label> _labels = new();

#pragma warning disable CS8618
    public Attachment()
#pragma warning restore CS8618
    {
    }

    public Attachment(string project, string parentType, Guid parentGuid, string fileName, Guid? proCoSysGuid = null)
    {
        ParentType = parentType;
        ParentGuid = parentGuid;
        FileName = fileName;
        Description = fileName;
        Guid = proCoSysGuid ?? MassTransit.NewId.NextGuid();
        BlobPath = Path.Combine(project, ParentType, Guid.ToString()).Replace("\\", "/");
        RevisionNumber = 1;
    }

    public IReadOnlyCollection<Label> Labels => _labels.AsReadOnly();
    public IOrderedEnumerable<Label> GetOrderedNonVoidedLabels()
        => _labels.Where(l => !l.IsVoided).OrderBy(l => l.Text);

    // private setters needed for Entity Framework
    public string ParentType { get; private set; }
    public Guid ParentGuid { get; private set; }
    public string FileName { get; private set; }
    public string Description { get; set; }
    public string BlobPath { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Person CreatedBy { get; private set; } = null!;
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Person? ModifiedBy { get; private set; }
    public Guid Guid { get; private set; }
    public int RevisionNumber { get; private set; }
    public DateTime ProCoSys4LastUpdated { get; set; }
    public string? ProCoSys4LastUpdatedByUser { get; set; }
    public DateTime SyncTimestamp { get; set; }

    public void IncreaseRevisionNumber() => RevisionNumber++;

    public string GetFullBlobPath()
        => Path.Combine(BlobPath, FileName).Replace("\\", "/");

    public void SetCreated(Person createdBy)
    {
        CreatedAtUtc = TimeService.UtcNow;
        CreatedById = createdBy.Id;
        CreatedBy = createdBy;
    }

    public void SetModified(Person modifiedBy)
    {
        ModifiedAtUtc = TimeService.UtcNow;
        ModifiedById = modifiedBy.Id;
        ModifiedBy = modifiedBy;
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

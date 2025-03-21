﻿using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

public class LibraryItem : PlantEntityBase, IAggregateRoot, IHaveGuid, IVoidable
{
    public const int CodeLengthMax = 255;
    public const int DescriptionLengthMax = 255;
    public const int TypeLengthMax = 30;

    private readonly List<Classification> _classifications = new();

#pragma warning disable CS8618
    protected LibraryItem()
#pragma warning restore CS8618
        : base(null)
    {
    }

    public LibraryItem(string plant, Guid guid, string code, string description, LibraryType type)
        : base(plant)
    {
        Guid = guid;
        Code = code;
        Description = description;
        Type = type;
    }

    // private setters needed for Entity Framework
    public Guid Guid { get; private set; }
    public string Code { get; set; }
    public string Description { get; set; }
    public LibraryType Type { get; set; }
    
    public bool IsVoided { get; set; }
    public DateTime ProCoSys4LastUpdated { get; set; }
    public DateTime SyncTimestamp { get; set; }

    public override string ToString() => $"{Code}, {Description}";

    public IReadOnlyCollection<Classification> Classifications => _classifications.AsReadOnly();

    public void AddClassification(Classification classification)
    {
        if(!ClassificationExistsOrNameInUse(classification))
        {
            _classifications.Add(classification);
        }
    }

    private bool ClassificationExistsOrNameInUse(Classification classification) => 
        _classifications.Any(c => c.Guid == classification.Guid || c.Name == classification.Name);

    public void RemoveClassification(Classification classification) => _classifications.Remove(classification);
}

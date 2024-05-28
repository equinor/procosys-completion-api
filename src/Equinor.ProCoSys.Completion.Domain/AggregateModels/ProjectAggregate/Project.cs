using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;

public class Project : PlantEntityBase, IAggregateRoot, IHaveGuid, IVoidable
{
    public const int NameLengthMax = 30;
    public const int DescriptionLengthMax = 1000;

    private bool _isDeletedInSource;

#pragma warning disable CS8618
    protected Project()
#pragma warning restore CS8618
        : base(null)
    {
    }

    public Project(string plant, Guid guid, string name, string description)
        : base(plant)
    {
        Guid = guid;
        Name = name;
        Description = description;
        ProCoSys4LastUpdated = DateTime.Now;
    }

    // private setters needed for Entity Framework
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsClosed { get; set; }
    
    /**
     * LastUpdated is the last time the project was updated in ProCoSys4
     * We do not save it as UTC, because we do not know for sure if it is UTC or not (it is, but we don't know for sure)
     */
    public DateTime ProCoSys4LastUpdated { get; set; }

    public DateTime SyncTimestamp { get; set; }
    public Guid Guid { get; private set; }
    
    public bool IsDeletedInSource
    {
        get => _isDeletedInSource;
        set
        {
            if (_isDeletedInSource && !value)
            {
                // this is an Undelete, which don't make sense
                throw new Exception("Changing IsDeletedInSource from true to false is not supported!");
            }

            // do nothing if already set
            if (_isDeletedInSource == value)
            {
                return;
            }

            _isDeletedInSource = value;

            // Make sure to close when setting _isDeletedInSource
            IsClosed = value;
        }
    }

    public bool IsVoided { get; set; }
}

using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Audit;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.ProjectAggregate;

public class Project : PlantEntityBase, IAggregateRoot, ICreationAuditable, IModificationAuditable, IHaveGuid
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
    }

    // private setters needed for Entity Framework
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsClosed { get; set; }
    public DateTime CreatedAtUtc { get; private set; }
    public int CreatedById { get; private set; }
    public Guid CreatedByOid { get; private set; }
    public DateTime? ModifiedAtUtc { get; private set; }
    public int? ModifiedById { get; private set; }
    public Guid? ModifiedByOid { get; private set; }
    public Guid Guid { get; private set; }

    public void SetCreated(Person createdBy)
    {
        CreatedAtUtc = TimeService.UtcNow;
        CreatedById = createdBy.Id;
        CreatedByOid = createdBy.Guid;
    }

    public void SetModified(Person modifiedBy)
    {
        ModifiedAtUtc = TimeService.UtcNow;
        ModifiedById = modifiedBy.Id;
        ModifiedByOid = modifiedBy.Guid;
    }

    public bool IsDeletedInSource
    {
        get => _isDeletedInSource;
        set
        {
            if (_isDeletedInSource && !value)
            {
                // this is an Undelete, which don't make sence
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
}

using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

public class Classification : EntityBase, IHaveGuid
{
    public const int NameLengthMax = 128;
    public const string PunchPriority = "PUNCH_PRIORITY";

    public Classification(Guid guid, string name)
    {
        Guid = guid;
        Name = name;
    }

    // private setters needed for Entity Framework
    public Guid Guid { get; private set; }
    public string Name { get; private set; }
}

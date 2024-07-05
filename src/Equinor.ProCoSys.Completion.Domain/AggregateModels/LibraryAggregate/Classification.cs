using System;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

public class Classification(Guid guid, string name) : IHaveGuid
{
    public const int NameLengthMax = 128;
    public const string PunchPriority = "PUNCH_PRIORITY";
    
    // private setters needed for Entity Framework
    public Guid Guid { get; private set; } = guid;
    public string Name { get; private set; } = name;
}

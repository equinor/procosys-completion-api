using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

namespace Equinor.ProCoSys.Completion.Domain;


public readonly record struct PersonKey(string Email);

public readonly record struct LibraryItemByPlant(string Code, LibraryType Type, string Plant);

public readonly record struct ProjectByPlantKey(string Project, string Plant);

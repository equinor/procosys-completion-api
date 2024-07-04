using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public static class StringExtension
{
    public static LibraryType ToLibraryType(this string libraryTypeStr) => Enum.Parse<LibraryType>(libraryTypeStr);
}

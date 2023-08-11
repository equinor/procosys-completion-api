using System;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Equinor.ProCoSys.Completion.Infrastructure;

public class LibraryTypeConverter : ValueConverter<LibraryType, string>
{
    public LibraryTypeConverter() : base(
        v => v.ToString(),
        v => (LibraryType)Enum.Parse(typeof(LibraryType), v))
    {}
}

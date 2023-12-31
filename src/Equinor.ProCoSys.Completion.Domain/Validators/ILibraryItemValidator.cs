﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LibraryAggregate;

namespace Equinor.ProCoSys.Completion.Domain.Validators;

public interface ILibraryItemValidator
{
    Task<bool> ExistsAsync(Guid libraryItemGuid, CancellationToken cancellationToken);
    Task<bool> HasTypeAsync(Guid libraryItemGuid, LibraryType type, CancellationToken cancellationToken);
    Task<bool> IsVoidedAsync(Guid libraryItemGuid, CancellationToken cancellationToken);
}

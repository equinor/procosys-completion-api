using Equinor.ProCoSys.Completion.Domain.Imports;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Models;

public readonly record struct ImportResult(
    Guid MessageGuid,   
    IEnumerable<ImportError> Errors);

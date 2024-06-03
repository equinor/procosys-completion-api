using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public interface IPropertyHelper
{
    User? GetPropertyValueAsUser(object? propertyValue, ValueDisplayType valueDisplayType);
}

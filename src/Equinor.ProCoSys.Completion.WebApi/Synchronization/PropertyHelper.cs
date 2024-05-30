using System;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class PropertyHelper(ILogger<PropertyHelper> logger) : IPropertyHelper
{
    public User? TryGetPropertyValueAsUser(object? propertyValue, ValueDisplayType valueDisplayType)
    {
        if (propertyValue is null || !valueDisplayType.ToString().StartsWith("User"))
        {
            return null;
        }

        var propertyValueAsString = propertyValue.ToString();
        try
        {
            var user = JsonConvert.DeserializeObject<User>(propertyValueAsString!);
            if (user is null)
            {
                throw new("DeserializeObject returned null");
            }
            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"Error deserialize {propertyValueAsString} into {nameof(User)}");
            throw;
        }
    }
}

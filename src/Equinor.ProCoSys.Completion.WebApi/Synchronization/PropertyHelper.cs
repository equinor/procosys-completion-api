using System;
// todo consider go through codebase and use System.Text.Json instead of Newtonsoft.Json
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class PropertyHelper(ILogger<PropertyHelper> logger) : IPropertyHelper
{
    public User? GetPropertyValueAsUser(object? propertyValue, ValueDisplayType valueDisplayType)
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
            logger.LogError(ex, "Error deserialize {property} into {userObject}", propertyValueAsString, nameof(User));
            throw;
        }
    }
}

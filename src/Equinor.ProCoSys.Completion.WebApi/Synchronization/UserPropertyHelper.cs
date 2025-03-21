﻿using System;
using System.Text.Json;
using Equinor.ProCoSys.Completion.MessageContracts;
using Equinor.ProCoSys.Completion.MessageContracts.History;
using Microsoft.Extensions.Logging;

namespace Equinor.ProCoSys.Completion.WebApi.Synchronization;

public class UserPropertyHelper(ILogger<UserPropertyHelper> logger) : IUserPropertyHelper
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
            var user = JsonSerializer.Deserialize<User>(
                propertyValueAsString!,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (user is null)
            {
                throw new("DeserializeObject returned null");
            }
            return user;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deserialize '{property}' into {objectName}", propertyValueAsString, nameof(User));
            throw;
        }
    }
}

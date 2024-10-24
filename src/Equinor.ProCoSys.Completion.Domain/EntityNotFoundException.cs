﻿using System;

namespace Equinor.ProCoSys.Completion.Domain;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException()
    {
    }
    public EntityNotFoundException(string message) : base(message)
    {
    }
}

public sealed class EntityNotFoundException<T>(Guid guid) : Exception($"Could not find {typeof(T).Name} with Guid {guid}");

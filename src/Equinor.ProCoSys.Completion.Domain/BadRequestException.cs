using System;

namespace Equinor.ProCoSys.Completion.Domain;

public sealed class BadRequestException(string message) : Exception(message);

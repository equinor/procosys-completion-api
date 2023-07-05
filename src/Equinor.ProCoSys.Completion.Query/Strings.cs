using System;

namespace Equinor.ProCoSys.Completion.Query;

public static class Strings
{
    public static string EntityNotFound(string entity, int id) => $"{entity} with ID {id} not found";
    public static string EntityNotFound(string entity, Guid guid) => $"{entity} with ID {guid} not found";
    public static string EntityNotFound(string entity, string name) => $"{entity} with Name {name} not found";
}

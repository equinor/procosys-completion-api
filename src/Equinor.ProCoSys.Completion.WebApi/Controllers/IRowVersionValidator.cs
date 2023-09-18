namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public interface IRowVersionValidator
{
    bool IsValid(string? rowVersion);
}

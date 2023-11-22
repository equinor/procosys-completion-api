namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public interface IRowVersionInputValidator
{
    bool IsValid(string rowVersion);
}

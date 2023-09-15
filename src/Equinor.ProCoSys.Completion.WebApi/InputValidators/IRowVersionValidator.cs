namespace Equinor.ProCoSys.Completion.WebApi.InputValidators;

public interface IRowVersionValidator
{
    bool IsValid(string? rowVersion);
}

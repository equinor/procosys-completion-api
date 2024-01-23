using System;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers;

public class RowVersionInputValidator : IRowVersionInputValidator
{
    public bool IsValid(string rowVersion)
        => !string.IsNullOrWhiteSpace(rowVersion) && TryConvertBase64StringToByteArray(rowVersion);

    private static bool TryConvertBase64StringToByteArray(string input)
    {
        try
        {
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Convert.FromBase64String(input);
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

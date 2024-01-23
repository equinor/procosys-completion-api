using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Misc;

public static class StringLengthAttributeExtension
{
    public static bool IsValid(this StringLengthAttribute attribute, string? value, out string? message)
    {
        message = null;
        if (attribute.MinimumLength == 0 && attribute.MaximumLength == 0)
        {
            return true;
        }

        var strLen = 0;
        if (value is not null)
        {
            strLen = value.Length;
        }

        if (attribute.MinimumLength > 0 && attribute.MaximumLength > 0)
        {
            if (strLen < attribute.MinimumLength || strLen > attribute.MaximumLength)
            {
                message =
                    $"Length is {strLen}. Length must be minimum {attribute.MinimumLength} and maximum {attribute.MaximumLength}";
            }
        }
        else if (attribute.MinimumLength > 0)
        {
            if (strLen < attribute.MinimumLength)
            {
                message =
                    $"Length is {strLen}. Length must be minimum {attribute.MinimumLength}";
            }
        }
        else if (attribute.MaximumLength > 0)
        {
            if (strLen > attribute.MaximumLength)
            {
                message =
                    $"Length is {strLen}. Length must be maximum {attribute.MaximumLength}";
            }
        }

        return message is null;
    }
}

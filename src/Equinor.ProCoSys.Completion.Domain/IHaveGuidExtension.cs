using System.Dynamic;
using Equinor.ProCoSys.Common;

namespace Equinor.ProCoSys.Completion.Domain;

public static class IHaveGuidExtension
{
    public static string GetContextName(this IHaveGuid haveGuid) => haveGuid.GetType().Name;

    public static dynamic GetEmailContext(this IHaveGuid haveGuid)
    {
        dynamic emailContext = new ExpandoObject();
        emailContext.Entity = haveGuid;
        return emailContext;
    }
}

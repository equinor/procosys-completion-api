using Equinor.ProCoSys.Auth.Authorization;

namespace Equinor.ProCoSys.Completion.WebApi;

public class Permissions
{
    // These soft strings like PUNCHLISTITEM/READ come from PCS4 Authorization model... 
    // ... We need to keep them as is. Do not change to PUNCHITEM/READ etc before the Authorization model ...
    // ... is rewritten for PCS5
    public const string PUNCHITEM_READ = "PUNCHLISTITEM/READ";
    public const string PUNCHITEM_CREATE = "PUNCHLISTITEM/CREATE";
    public const string PUNCHITEM_DELETE = "PUNCHLISTITEM/DELETE";
    public const string PUNCHITEM_WRITE = "PUNCHLISTITEM/WRITE";
    public const string PUNCHITEM_ATTACH = "PUNCHLISTITEM/ATTACH";
    public const string PUNCHITEM_DETACH = "PUNCHLISTITEM/DETACH";
    public const string PUNCHITEM_CLEAR = "PUNCHLISTITEM/CLEAR";
    public const string PUNCHITEM_VERIFY = "PUNCHLISTITEM/VERIFY";
    public const string USER_READ = "USER/READ";

    public const string LIBRARY_READ = "LIBRARY_GENERAL/READ";

    public const string APPLICATION_TESTER = "APPLICATION_EXPLORER/EXECUTE";

    public const string SUPERUSER = ClaimsTransformation.Superuser;
}

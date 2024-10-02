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
    public const string PUNCHITEM_CLEAR = "PUNCHLISTITEM/CLEAR";
    public const string PUNCHITEM_VERIFY = "PUNCHLISTITEM/VERIFY";
    public const string USER_READ = "USER/READ";
    public const string WO_READ = "WO/READ";
    public const string SWCR_READ = "SWCR/READ";
    public const string DOCUMENT_READ = "DOCUMENT/READ";
    public const string MCCR_READ = "MCCR/READ";
    public const string CPCL_READ = "CPCL/READ";
    public const string DCCL_READ = "DCCL/READ";

    public const string APPLICATION_TESTER = "APPLICATION_EXPLORER/EXECUTE";

    public const string SUPERUSER = ClaimsTransformation.Superuser;
}

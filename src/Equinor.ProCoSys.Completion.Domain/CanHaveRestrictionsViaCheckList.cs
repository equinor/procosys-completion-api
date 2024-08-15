using System;

namespace Equinor.ProCoSys.Completion.Domain;

// abstract class to be used for all MediatR commands (write) which need check if user has restrictions via checklist
// In ProCoSys 4, it is possible to add a so-called Restrictions Role to users. If user has such role, the role define which
// responsible code the user is restricted to. Restrictions Role are only affecting writing data (i.e. when executing MediatR
// commands), not reading data (i.e. not when executing MediatR queries).
//
// Sample: If user has Restrictions Role for responsible code RespA and RespB, the user should be denied changing data on
// objects with a responsible, and the responsible is other than RespA or RespB. A checklist has a responsible. Punch responsible
// is found via the checklist owning the punch
public abstract class CanHaveRestrictionsViaCheckList : NeedProjectAccess
{
    public abstract Guid GetCheckListGuidForWriteAccessCheck();
}

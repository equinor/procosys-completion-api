﻿using System;
using System.Collections.Generic;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PunchItemCommands.DuplicatePunchItem;

public class DuplicatePunchItemCommand(Guid punchItemGuid, List<Guid> checkListGuids, bool duplicateAttachments) 
    : ICanHaveRestrictionsViaCheckLists, IRequest<Result<List<GuidAndRowVersion>>>, IIsPunchItemCommand
{
    public Guid PunchItemGuid { get; } = punchItemGuid;
    public List<Guid> CheckListGuids { get; } = checkListGuids;
    public bool DuplicateAttachments { get; } = duplicateAttachments;
    public PunchItem PunchItem { get; set; } = null!;
    public Guid GetProjectGuidForAccessCheck() => PunchItem.Project.Guid;
    public List<Guid> GetCheckListGuidsForWriteAccessCheck() => CheckListGuids;
    public List<CheckListDetailsDto> CheckListDetailsDtos { get; set; } = null!;
}
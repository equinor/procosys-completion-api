using System;
using Equinor.ProCoSys.Completion.Domain.Events.DomainEvents.PunchEvents;
using Equinor.ProCoSys.MessageContracts.Punch;

namespace Equinor.ProCoSys.Completion.Command.EventHandlers.DomainEvents.PunchEvents;
public record PunchCreatedMessage : IPunchCreatedV1
    {
        internal PunchCreatedMessage(PunchCreatedEvent punchCreatedEvent)
        {
            ProjectGuid = punchCreatedEvent.ProjectGuid;
            Guid = punchCreatedEvent.Punch.Guid;
            ItemNo = punchCreatedEvent.Punch.ItemNo;
            CreatedAtUtc = punchCreatedEvent.Punch.CreatedAtUtc;
            CreatedByOid = punchCreatedEvent.Punch.CreatedByOid;
        }

        public string DisplayName => "Punch created";

        public Guid ProjectGuid { get; }
        public Guid Guid { get; init; }
        public string ItemNo { get; init; }
        public Guid CreatedByOid { get; init; }
        public DateTime CreatedAtUtc { get; init; }
    }


﻿using System;
using System.Collections.Generic;

namespace Equinor.ProCoSys.Completion.MessageContracts;

public interface IPunchItemUpdatedV1 : IPunchItem, IIntegrationEvent
{
    Guid ModifiedByOid { get; }
    DateTime ModifiedAtUtc { get; }
    List<IProperty> Changes { get; }
}
﻿namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public class SyncObjectDto
{
    public required string SyncObjectName { get; set; }
    public required object SyncObject { get; set; }
    public required string SyncPlant { get; set; }
}
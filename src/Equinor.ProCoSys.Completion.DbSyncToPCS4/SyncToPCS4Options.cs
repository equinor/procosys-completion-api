﻿namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public class SyncToPCS4Options
{
    public required string Endpoint { get; set; }
    public required bool Enabled { get; set; }
    public required string Scope { get; set; }
}

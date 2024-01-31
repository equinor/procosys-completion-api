IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE TABLE [InboxState] (
        [Id] bigint NOT NULL IDENTITY,
        [MessageId] uniqueidentifier NOT NULL,
        [ConsumerId] uniqueidentifier NOT NULL,
        [LockId] uniqueidentifier NOT NULL,
        [RowVersion] rowversion NULL,
        [Received] datetime2 NOT NULL,
        [ReceiveCount] int NOT NULL,
        [ExpirationTime] datetime2 NULL,
        [Consumed] datetime2 NULL,
        [Delivered] datetime2 NULL,
        [LastSequenceNumber] bigint NULL,
        CONSTRAINT [PK_InboxState] PRIMARY KEY ([Id]),
        CONSTRAINT [AK_InboxState_MessageId_ConsumerId] UNIQUE ([MessageId], [ConsumerId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE TABLE [OutboxMessage] (
        [SequenceNumber] bigint NOT NULL IDENTITY,
        [EnqueueTime] datetime2 NULL,
        [SentTime] datetime2 NOT NULL,
        [Headers] nvarchar(max) NULL,
        [Properties] nvarchar(max) NULL,
        [InboxMessageId] uniqueidentifier NULL,
        [InboxConsumerId] uniqueidentifier NULL,
        [OutboxId] uniqueidentifier NULL,
        [MessageId] uniqueidentifier NOT NULL,
        [ContentType] nvarchar(256) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [ConversationId] uniqueidentifier NULL,
        [CorrelationId] uniqueidentifier NULL,
        [InitiatorId] uniqueidentifier NULL,
        [RequestId] uniqueidentifier NULL,
        [SourceAddress] nvarchar(256) NULL,
        [DestinationAddress] nvarchar(256) NULL,
        [ResponseAddress] nvarchar(256) NULL,
        [FaultAddress] nvarchar(256) NULL,
        [ExpirationTime] datetime2 NULL,
        CONSTRAINT [PK_OutboxMessage] PRIMARY KEY ([SequenceNumber])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE TABLE [OutboxState] (
        [OutboxId] uniqueidentifier NOT NULL,
        [LockId] uniqueidentifier NOT NULL,
        [RowVersion] rowversion NULL,
        [Created] datetime2 NOT NULL,
        [Delivered] datetime2 NULL,
        [LastSequenceNumber] bigint NULL,
        CONSTRAINT [PK_OutboxState] PRIMARY KEY ([OutboxId])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [Persons] (
        [Id] int NOT NULL IDENTITY,
        [Guid] uniqueidentifier NOT NULL,
        [FirstName] nvarchar(128) NOT NULL,
        [LastName] nvarchar(128) NOT NULL,
        [UserName] nvarchar(128) NOT NULL,
        [Email] nvarchar(128) NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [ModifiedByOid] uniqueidentifier NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Persons] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Persons_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[PersonsHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [Attachments] (
        [Id] int NOT NULL IDENTITY,
        [SourceType] nvarchar(256) NOT NULL,
        [SourceGuid] uniqueidentifier NOT NULL,
        [FileName] nvarchar(255) NOT NULL,
        [BlobPath] nvarchar(1024) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [CreatedByOid] uniqueidentifier NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [ModifiedByOid] uniqueidentifier NULL,
        [Guid] uniqueidentifier NOT NULL,
        [RevisionNumber] int NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Attachments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Attachments_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[AttachmentsHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [Comments] (
        [Id] int NOT NULL IDENTITY,
        [SourceType] nvarchar(256) NOT NULL,
        [Text] nvarchar(4000) NOT NULL,
        [SourceGuid] uniqueidentifier NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [CreatedByOid] uniqueidentifier NOT NULL,
        [Guid] uniqueidentifier NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Comments] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Comments_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[CommentsHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [Links] (
        [Id] int NOT NULL IDENTITY,
        [SourceType] nvarchar(256) NOT NULL,
        [SourceGuid] uniqueidentifier NOT NULL,
        [Title] nvarchar(256) NOT NULL,
        [Url] nvarchar(2000) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [CreatedByOid] uniqueidentifier NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [ModifiedByOid] uniqueidentifier NULL,
        [Guid] uniqueidentifier NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Links] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Links_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_Links_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[LinksHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [Projects] (
        [Id] int NOT NULL IDENTITY,
        [Name] nvarchar(30) NOT NULL,
        [Description] nvarchar(1000) NOT NULL,
        [IsClosed] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [CreatedByOid] uniqueidentifier NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [ModifiedByOid] uniqueidentifier NULL,
        [Guid] uniqueidentifier NOT NULL,
        [IsDeletedInSource] bit NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        [Plant] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_Projects] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Projects_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_Projects_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[ProjectsHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [PunchItems] (
        [Id] int NOT NULL IDENTITY(4000001, 1),
        [ProjectId] int NOT NULL,
        [Description] nvarchar(2000) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [CreatedByOid] uniqueidentifier NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [ModifiedByOid] uniqueidentifier NULL,
        [Guid] uniqueidentifier NOT NULL,
        [ClearedAtUtc] datetime2 NULL,
        [ClearedById] int NULL,
        [RejectedAtUtc] datetime2 NULL,
        [RejectedById] int NULL,
        [VerifiedAtUtc] datetime2 NULL,
        [VerifiedById] int NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        [Plant] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_PunchItems] PRIMARY KEY ([Id]),
        CONSTRAINT [punch_item_check_cleared] CHECK ((ClearedAtUtc is null and ClearedById is null) or (ClearedAtUtc is not null and ClearedById is not null)),
        CONSTRAINT [punch_item_check_cleared_rejected] CHECK (not (ClearedAtUtc is not null and RejectedAtUtc is not null)),
        CONSTRAINT [punch_item_check_cleared_verified] CHECK (not (ClearedAtUtc is null and VerifiedAtUtc is not null)),
        CONSTRAINT [punch_item_check_rejected] CHECK ((RejectedAtUtc is null and RejectedById is null) or (RejectedAtUtc is not null and RejectedById is not null)),
        CONSTRAINT [punch_item_check_verified] CHECK ((VerifiedAtUtc is null and VerifiedById is null) or (VerifiedAtUtc is not null and VerifiedById is not null)),
        CONSTRAINT [FK_PunchItems_Persons_ClearedById] FOREIGN KEY ([ClearedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_PunchItems_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_PunchItems_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_PunchItems_Persons_RejectedById] FOREIGN KEY ([RejectedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_PunchItems_Persons_VerifiedById] FOREIGN KEY ([VerifiedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_PunchItems_Projects_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [Projects] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[PunchItemsHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Attachments_CreatedById] ON [Attachments] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Attachments_SourceGuid] ON [Attachments] ([SourceGuid]) INCLUDE ([Guid], [FileName], [CreatedById], [CreatedAtUtc], [ModifiedById], [ModifiedAtUtc], [RowVersion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Comments_CreatedById] ON [Comments] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Comments_SourceGuid] ON [Comments] ([SourceGuid]) INCLUDE ([Guid], [Text], [CreatedById], [CreatedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_InboxState_Delivered] ON [InboxState] ([Delivered]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Links_CreatedById] ON [Links] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Links_ModifiedById] ON [Links] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Links_SourceGuid] ON [Links] ([SourceGuid]) INCLUDE ([Guid], [Url], [Title], [RowVersion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_OutboxMessage_EnqueueTime] ON [OutboxMessage] ([EnqueueTime]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_OutboxMessage_ExpirationTime] ON [OutboxMessage] ([ExpirationTime]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber] ON [OutboxMessage] ([InboxMessageId], [InboxConsumerId], [SequenceNumber]) WHERE [InboxMessageId] IS NOT NULL AND [InboxConsumerId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_OutboxMessage_OutboxId_SequenceNumber] ON [OutboxMessage] ([OutboxId], [SequenceNumber]) WHERE [OutboxId] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_OutboxState_Created] ON [OutboxState] ([Created]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Persons_Guid] ON [Persons] ([Guid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Persons_ModifiedById] ON [Persons] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Projects_CreatedById] ON [Projects] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Projects_ModifiedById] ON [Projects] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Projects_Name_ASC] ON [Projects] ([Name]) INCLUDE ([Plant]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_Projects_Plant_ASC] ON [Projects] ([Plant]) INCLUDE ([Name], [IsClosed], [CreatedAtUtc], [ModifiedAtUtc]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PunchItems_ClearedById] ON [PunchItems] ([ClearedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PunchItems_CreatedById] ON [PunchItems] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PunchItems_Guid] ON [PunchItems] ([Guid]) INCLUDE ([Id], [Description], [ProjectId], [CreatedById], [CreatedAtUtc], [ModifiedById], [ModifiedAtUtc], [ClearedById], [ClearedAtUtc], [VerifiedById], [VerifiedAtUtc], [RejectedById], [RejectedAtUtc], [RowVersion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PunchItems_ModifiedById] ON [PunchItems] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PunchItems_ProjectId] ON [PunchItems] ([ProjectId]) INCLUDE ([Id], [RowVersion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PunchItems_RejectedById] ON [PunchItems] ([RejectedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    CREATE INDEX [IX_PunchItems_VerifiedById] ON [PunchItems] ([VerifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801064035_InitialMigration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230801064035_InitialMigration', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801122528_Library'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [Library] (
        [Id] int NOT NULL IDENTITY,
        [Guid] uniqueidentifier NOT NULL,
        [Code] nvarchar(255) NOT NULL,
        [Description] nvarchar(255) NOT NULL,
        [Type] nvarchar(30) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [CreatedByOid] uniqueidentifier NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [ModifiedByOid] uniqueidentifier NULL,
        [IsVoided] bit NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        [Plant] nvarchar(max) NULL,
        CONSTRAINT [PK_Library] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Library_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_Library_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[LibraryHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801122528_Library'
)
BEGIN
    CREATE INDEX [IX_Library_CreatedById] ON [Library] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801122528_Library'
)
BEGIN
    CREATE INDEX [IX_Library_ModifiedById] ON [Library] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801122528_Library'
)
BEGIN
    CREATE INDEX [IX_LibraryItems_Guid] ON [Library] ([Guid]) INCLUDE ([Code], [Description], [Type]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230801122528_Library'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230801122528_Library', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    DROP INDEX [IX_PunchItems_Guid] ON [PunchItems];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [ClearingByOrgId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [PriorityId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [RaisedByOrgId] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [SortingId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [TypeId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    CREATE INDEX [IX_PunchItems_ClearingByOrgId] ON [PunchItems] ([ClearingByOrgId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    CREATE INDEX [IX_PunchItems_Guid] ON [PunchItems] ([Guid]) INCLUDE ([Id], [Description], [ProjectId], [CreatedById], [CreatedAtUtc], [ModifiedById], [ModifiedAtUtc], [ClearedById], [ClearedAtUtc], [VerifiedById], [VerifiedAtUtc], [RejectedById], [RejectedAtUtc], [RaisedByOrgId], [ClearingByOrgId], [SortingId], [TypeId], [PriorityId], [RowVersion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    CREATE INDEX [IX_PunchItems_PriorityId] ON [PunchItems] ([PriorityId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    CREATE INDEX [IX_PunchItems_RaisedByOrgId] ON [PunchItems] ([RaisedByOrgId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    CREATE INDEX [IX_PunchItems_SortingId] ON [PunchItems] ([SortingId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    CREATE INDEX [IX_PunchItems_TypeId] ON [PunchItems] ([TypeId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    ALTER TABLE [PunchItems] ADD CONSTRAINT [FK_PunchItems_Library_ClearingByOrgId] FOREIGN KEY ([ClearingByOrgId]) REFERENCES [Library] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    ALTER TABLE [PunchItems] ADD CONSTRAINT [FK_PunchItems_Library_PriorityId] FOREIGN KEY ([PriorityId]) REFERENCES [Library] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    ALTER TABLE [PunchItems] ADD CONSTRAINT [FK_PunchItems_Library_RaisedByOrgId] FOREIGN KEY ([RaisedByOrgId]) REFERENCES [Library] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    ALTER TABLE [PunchItems] ADD CONSTRAINT [FK_PunchItems_Library_SortingId] FOREIGN KEY ([SortingId]) REFERENCES [Library] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    ALTER TABLE [PunchItems] ADD CONSTRAINT [FK_PunchItems_Library_TypeId] FOREIGN KEY ([TypeId]) REFERENCES [Library] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230803062141_PunchItemLibraryRefs'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230803062141_PunchItemLibraryRefs', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230807101004_UniqueGuidIndex'
)
BEGIN
    DROP INDEX [IX_PunchItems_Guid] ON [PunchItems];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230807101004_UniqueGuidIndex'
)
BEGIN
    DROP INDEX [IX_Projects_Name_ASC] ON [Projects];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230807101004_UniqueGuidIndex'
)
BEGIN
    DROP INDEX [IX_Projects_Plant_ASC] ON [Projects];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230807101004_UniqueGuidIndex'
)
BEGIN
    DROP INDEX [IX_LibraryItems_Guid] ON [Library];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230807101004_UniqueGuidIndex'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PunchItems_Guid] ON [PunchItems] ([Guid]) INCLUDE ([Id], [Description], [ProjectId], [CreatedById], [CreatedAtUtc], [ModifiedById], [ModifiedAtUtc], [ClearedById], [ClearedAtUtc], [VerifiedById], [VerifiedAtUtc], [RejectedById], [RejectedAtUtc], [RaisedByOrgId], [ClearingByOrgId], [SortingId], [TypeId], [PriorityId], [RowVersion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230807101004_UniqueGuidIndex'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Projects_Guid] ON [Projects] ([Guid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230807101004_UniqueGuidIndex'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Links_Guid] ON [Links] ([Guid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230807101004_UniqueGuidIndex'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LibraryItems_Guid] ON [Library] ([Guid]) INCLUDE ([Code], [Description], [Type]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230807101004_UniqueGuidIndex'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Comments_Guid] ON [Comments] ([Guid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230807101004_UniqueGuidIndex'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Attachments_Guid] ON [Attachments] ([Guid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230807101004_UniqueGuidIndex'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230807101004_UniqueGuidIndex', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230810052314_CheckListGuidInPunchItem'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [CheckListGuid] uniqueidentifier NOT NULL DEFAULT '00000000-0000-0000-0000-000000000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230810052314_CheckListGuidInPunchItem'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230810052314_CheckListGuidInPunchItem', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230922091555_PunchWithNavigationProperties'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230922091555_PunchWithNavigationProperties', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    ALTER TABLE [PunchItems] SET (SYSTEM_VERSIONING = OFF)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var0 sysname;
    SELECT @var0 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PunchItems]') AND [c].[name] = N'CreatedByOid');
    IF @var0 IS NOT NULL EXEC(N'ALTER TABLE [PunchItems] DROP CONSTRAINT [' + @var0 + '];');
    ALTER TABLE [PunchItems] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var1 sysname;
    SELECT @var1 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PunchItemsHistory]') AND [c].[name] = N'CreatedByOid');
    IF @var1 IS NOT NULL EXEC(N'ALTER TABLE [PunchItemsHistory] DROP CONSTRAINT [' + @var1 + '];');
    ALTER TABLE [PunchItemsHistory] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var2 sysname;
    SELECT @var2 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PunchItems]') AND [c].[name] = N'ModifiedByOid');
    IF @var2 IS NOT NULL EXEC(N'ALTER TABLE [PunchItems] DROP CONSTRAINT [' + @var2 + '];');
    ALTER TABLE [PunchItems] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var3 sysname;
    SELECT @var3 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PunchItemsHistory]') AND [c].[name] = N'ModifiedByOid');
    IF @var3 IS NOT NULL EXEC(N'ALTER TABLE [PunchItemsHistory] DROP CONSTRAINT [' + @var3 + '];');
    ALTER TABLE [PunchItemsHistory] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    ALTER TABLE [Projects] SET (SYSTEM_VERSIONING = OFF)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var4 sysname;
    SELECT @var4 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'CreatedByOid');
    IF @var4 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var4 + '];');
    ALTER TABLE [Projects] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var5 sysname;
    SELECT @var5 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectsHistory]') AND [c].[name] = N'CreatedByOid');
    IF @var5 IS NOT NULL EXEC(N'ALTER TABLE [ProjectsHistory] DROP CONSTRAINT [' + @var5 + '];');
    ALTER TABLE [ProjectsHistory] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var6 sysname;
    SELECT @var6 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Projects]') AND [c].[name] = N'ModifiedByOid');
    IF @var6 IS NOT NULL EXEC(N'ALTER TABLE [Projects] DROP CONSTRAINT [' + @var6 + '];');
    ALTER TABLE [Projects] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var7 sysname;
    SELECT @var7 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[ProjectsHistory]') AND [c].[name] = N'ModifiedByOid');
    IF @var7 IS NOT NULL EXEC(N'ALTER TABLE [ProjectsHistory] DROP CONSTRAINT [' + @var7 + '];');
    ALTER TABLE [ProjectsHistory] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    ALTER TABLE [Persons] SET (SYSTEM_VERSIONING = OFF)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var8 sysname;
    SELECT @var8 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Persons]') AND [c].[name] = N'ModifiedByOid');
    IF @var8 IS NOT NULL EXEC(N'ALTER TABLE [Persons] DROP CONSTRAINT [' + @var8 + '];');
    ALTER TABLE [Persons] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var9 sysname;
    SELECT @var9 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[PersonsHistory]') AND [c].[name] = N'ModifiedByOid');
    IF @var9 IS NOT NULL EXEC(N'ALTER TABLE [PersonsHistory] DROP CONSTRAINT [' + @var9 + '];');
    ALTER TABLE [PersonsHistory] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    ALTER TABLE [Links] SET (SYSTEM_VERSIONING = OFF)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var10 sysname;
    SELECT @var10 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Links]') AND [c].[name] = N'CreatedByOid');
    IF @var10 IS NOT NULL EXEC(N'ALTER TABLE [Links] DROP CONSTRAINT [' + @var10 + '];');
    ALTER TABLE [Links] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var11 sysname;
    SELECT @var11 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LinksHistory]') AND [c].[name] = N'CreatedByOid');
    IF @var11 IS NOT NULL EXEC(N'ALTER TABLE [LinksHistory] DROP CONSTRAINT [' + @var11 + '];');
    ALTER TABLE [LinksHistory] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var12 sysname;
    SELECT @var12 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Links]') AND [c].[name] = N'ModifiedByOid');
    IF @var12 IS NOT NULL EXEC(N'ALTER TABLE [Links] DROP CONSTRAINT [' + @var12 + '];');
    ALTER TABLE [Links] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var13 sysname;
    SELECT @var13 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LinksHistory]') AND [c].[name] = N'ModifiedByOid');
    IF @var13 IS NOT NULL EXEC(N'ALTER TABLE [LinksHistory] DROP CONSTRAINT [' + @var13 + '];');
    ALTER TABLE [LinksHistory] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    ALTER TABLE [Library] SET (SYSTEM_VERSIONING = OFF)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var14 sysname;
    SELECT @var14 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Library]') AND [c].[name] = N'CreatedByOid');
    IF @var14 IS NOT NULL EXEC(N'ALTER TABLE [Library] DROP CONSTRAINT [' + @var14 + '];');
    ALTER TABLE [Library] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var15 sysname;
    SELECT @var15 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LibraryHistory]') AND [c].[name] = N'CreatedByOid');
    IF @var15 IS NOT NULL EXEC(N'ALTER TABLE [LibraryHistory] DROP CONSTRAINT [' + @var15 + '];');
    ALTER TABLE [LibraryHistory] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var16 sysname;
    SELECT @var16 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Library]') AND [c].[name] = N'ModifiedByOid');
    IF @var16 IS NOT NULL EXEC(N'ALTER TABLE [Library] DROP CONSTRAINT [' + @var16 + '];');
    ALTER TABLE [Library] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var17 sysname;
    SELECT @var17 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[LibraryHistory]') AND [c].[name] = N'ModifiedByOid');
    IF @var17 IS NOT NULL EXEC(N'ALTER TABLE [LibraryHistory] DROP CONSTRAINT [' + @var17 + '];');
    ALTER TABLE [LibraryHistory] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    ALTER TABLE [Comments] SET (SYSTEM_VERSIONING = OFF)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var18 sysname;
    SELECT @var18 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Comments]') AND [c].[name] = N'CreatedByOid');
    IF @var18 IS NOT NULL EXEC(N'ALTER TABLE [Comments] DROP CONSTRAINT [' + @var18 + '];');
    ALTER TABLE [Comments] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var19 sysname;
    SELECT @var19 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[CommentsHistory]') AND [c].[name] = N'CreatedByOid');
    IF @var19 IS NOT NULL EXEC(N'ALTER TABLE [CommentsHistory] DROP CONSTRAINT [' + @var19 + '];');
    ALTER TABLE [CommentsHistory] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    ALTER TABLE [Attachments] SET (SYSTEM_VERSIONING = OFF)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var20 sysname;
    SELECT @var20 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attachments]') AND [c].[name] = N'CreatedByOid');
    IF @var20 IS NOT NULL EXEC(N'ALTER TABLE [Attachments] DROP CONSTRAINT [' + @var20 + '];');
    ALTER TABLE [Attachments] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var21 sysname;
    SELECT @var21 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AttachmentsHistory]') AND [c].[name] = N'CreatedByOid');
    IF @var21 IS NOT NULL EXEC(N'ALTER TABLE [AttachmentsHistory] DROP CONSTRAINT [' + @var21 + '];');
    ALTER TABLE [AttachmentsHistory] DROP COLUMN [CreatedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var22 sysname;
    SELECT @var22 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Attachments]') AND [c].[name] = N'ModifiedByOid');
    IF @var22 IS NOT NULL EXEC(N'ALTER TABLE [Attachments] DROP CONSTRAINT [' + @var22 + '];');
    ALTER TABLE [Attachments] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @var23 sysname;
    SELECT @var23 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[AttachmentsHistory]') AND [c].[name] = N'ModifiedByOid');
    IF @var23 IS NOT NULL EXEC(N'ALTER TABLE [AttachmentsHistory] DROP CONSTRAINT [' + @var23 + '];');
    ALTER TABLE [AttachmentsHistory] DROP COLUMN [ModifiedByOid];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    CREATE INDEX [IX_Attachments_ModifiedById] ON [Attachments] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    ALTER TABLE [Attachments] ADD CONSTRAINT [FK_Attachments_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'ALTER TABLE [PunchItems] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[PunchItemsHistory]))')
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'ALTER TABLE [Projects] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[ProjectsHistory]))')
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'ALTER TABLE [Persons] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[PersonsHistory]))')
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'ALTER TABLE [Links] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[LinksHistory]))')
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'ALTER TABLE [Library] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[LibraryHistory]))')
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'ALTER TABLE [Comments] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[CommentsHistory]))')
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'ALTER TABLE [Attachments] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[AttachmentsHistory]))')
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230926073437_NavigationPropertiesToPerson'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230926073437_NavigationPropertiesToPerson', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    DROP INDEX [IX_PunchItems_Guid] ON [PunchItems];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [ActionById] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [Category] int NOT NULL DEFAULT 0;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [DocumentId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [DueDate] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [Estimate] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [ExternalItemNo] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [MaterialETA] datetime2 NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [MaterialExternalNo] nvarchar(100) NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [MaterialRequired] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [OriginalWorkOrderId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [SWCRId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD [WorkOrderId] int NULL;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [Documents] (
        [Id] int NOT NULL IDENTITY,
        [No] nvarchar(60) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [Guid] uniqueidentifier NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        [Plant] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_Documents] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Documents_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_Documents_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[DocumentsHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [SWCRs] (
        [Id] int NOT NULL IDENTITY,
        [No] int NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [Guid] uniqueidentifier NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        [Plant] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_SWCRs] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_SWCRs_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_SWCRs_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[SWCRsHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [WorkOrders] (
        [Id] int NOT NULL IDENTITY,
        [No] nvarchar(30) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [Guid] uniqueidentifier NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        [Plant] nvarchar(255) NOT NULL,
        CONSTRAINT [PK_WorkOrders] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_WorkOrders_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_WorkOrders_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[WorkOrdersHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE INDEX [IX_PunchItems_ActionById] ON [PunchItems] ([ActionById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE INDEX [IX_PunchItems_DocumentId] ON [PunchItems] ([DocumentId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PunchItems_Guid] ON [PunchItems] ([Guid]) INCLUDE ([Id], [Category], [Description], [ProjectId], [CreatedById], [CreatedAtUtc], [ModifiedById], [ModifiedAtUtc], [ClearedById], [ClearedAtUtc], [VerifiedById], [VerifiedAtUtc], [RejectedById], [RejectedAtUtc], [RaisedByOrgId], [ClearingByOrgId], [SortingId], [TypeId], [PriorityId], [Estimate], [DueDate], [ExternalItemNo], [MaterialRequired], [MaterialExternalNo], [MaterialETA], [WorkOrderId], [OriginalWorkOrderId], [DocumentId], [SWCRId], [ActionById], [RowVersion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE INDEX [IX_PunchItems_OriginalWorkOrderId] ON [PunchItems] ([OriginalWorkOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE INDEX [IX_PunchItems_SWCRId] ON [PunchItems] ([SWCRId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE INDEX [IX_PunchItems_WorkOrderId] ON [PunchItems] ([WorkOrderId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    EXEC(N'ALTER TABLE [PunchItems] ADD CONSTRAINT [punch_item_valid_category] CHECK (Category in (0,1))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE INDEX [IX_Documents_CreatedById] ON [Documents] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Documents_Guid] ON [Documents] ([Guid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE INDEX [IX_Documents_ModifiedById] ON [Documents] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE INDEX [IX_SWCRs_CreatedById] ON [SWCRs] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE UNIQUE INDEX [IX_SWCRs_Guid] ON [SWCRs] ([Guid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE INDEX [IX_SWCRs_ModifiedById] ON [SWCRs] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE INDEX [IX_WorkOrders_CreatedById] ON [WorkOrders] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE UNIQUE INDEX [IX_WorkOrders_Guid] ON [WorkOrders] ([Guid]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    CREATE INDEX [IX_WorkOrders_ModifiedById] ON [WorkOrders] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD CONSTRAINT [FK_PunchItems_Documents_DocumentId] FOREIGN KEY ([DocumentId]) REFERENCES [Documents] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD CONSTRAINT [FK_PunchItems_Persons_ActionById] FOREIGN KEY ([ActionById]) REFERENCES [Persons] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD CONSTRAINT [FK_PunchItems_SWCRs_SWCRId] FOREIGN KEY ([SWCRId]) REFERENCES [SWCRs] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD CONSTRAINT [FK_PunchItems_WorkOrders_OriginalWorkOrderId] FOREIGN KEY ([OriginalWorkOrderId]) REFERENCES [WorkOrders] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    ALTER TABLE [PunchItems] ADD CONSTRAINT [FK_PunchItems_WorkOrders_WorkOrderId] FOREIGN KEY ([WorkOrderId]) REFERENCES [WorkOrders] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927054535_PunchRemainingProps'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230927054535_PunchRemainingProps', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927101147_IsVoidedProps'
)
BEGIN
    ALTER TABLE [WorkOrders] ADD [IsClosed] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927101147_IsVoidedProps'
)
BEGIN
    ALTER TABLE [SWCRs] ADD [IsVoided] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927101147_IsVoidedProps'
)
BEGIN
    ALTER TABLE [Documents] ADD [IsVoided] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927101147_IsVoidedProps'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230927101147_IsVoidedProps', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927125159_DateTimesInUTC'
)
BEGIN
    DROP INDEX [IX_PunchItems_Guid] ON [PunchItems];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927125159_DateTimesInUTC'
)
BEGIN
    EXEC sp_rename N'[PunchItems].[MaterialETA]', N'MaterialETAUtc', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927125159_DateTimesInUTC'
)
BEGIN
    EXEC sp_rename N'[PunchItems].[DueDate]', N'DueTimeUtc', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927125159_DateTimesInUTC'
)
BEGIN
    CREATE UNIQUE INDEX [IX_PunchItems_Guid] ON [PunchItems] ([Guid]) INCLUDE ([Id], [Category], [Description], [ProjectId], [CreatedById], [CreatedAtUtc], [ModifiedById], [ModifiedAtUtc], [ClearedById], [ClearedAtUtc], [VerifiedById], [VerifiedAtUtc], [RejectedById], [RejectedAtUtc], [RaisedByOrgId], [ClearingByOrgId], [SortingId], [TypeId], [PriorityId], [Estimate], [DueTimeUtc], [ExternalItemNo], [MaterialRequired], [MaterialExternalNo], [MaterialETAUtc], [WorkOrderId], [OriginalWorkOrderId], [DocumentId], [SWCRId], [ActionById], [RowVersion]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20230927125159_DateTimesInUTC'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20230927125159_DateTimesInUTC', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231107064234_RenameSourceGuid'
)
BEGIN
    EXEC sp_rename N'[Links].[SourceType]', N'ParentType', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231107064234_RenameSourceGuid'
)
BEGIN
    EXEC sp_rename N'[Links].[SourceGuid]', N'ParentGuid', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231107064234_RenameSourceGuid'
)
BEGIN
    EXEC sp_rename N'[Links].[IX_Links_SourceGuid]', N'IX_Links_ParentGuid', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231107064234_RenameSourceGuid'
)
BEGIN
    EXEC sp_rename N'[Comments].[SourceType]', N'ParentType', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231107064234_RenameSourceGuid'
)
BEGIN
    EXEC sp_rename N'[Comments].[SourceGuid]', N'ParentGuid', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231107064234_RenameSourceGuid'
)
BEGIN
    EXEC sp_rename N'[Comments].[IX_Comments_SourceGuid]', N'IX_Comments_ParentGuid', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231107064234_RenameSourceGuid'
)
BEGIN
    EXEC sp_rename N'[Attachments].[SourceType]', N'ParentType', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231107064234_RenameSourceGuid'
)
BEGIN
    EXEC sp_rename N'[Attachments].[SourceGuid]', N'ParentGuid', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231107064234_RenameSourceGuid'
)
BEGIN
    EXEC sp_rename N'[Attachments].[IX_Attachments_SourceGuid]', N'IX_Attachments_ParentGuid', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231107064234_RenameSourceGuid'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231107064234_RenameSourceGuid', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231120123136_Labels'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [LabelHost] (
        [Id] int NOT NULL IDENTITY,
        [Type] nvarchar(450) NOT NULL DEFAULT N''GeneralPicture'',
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_LabelHost] PRIMARY KEY ([Id]),
        CONSTRAINT [host_type_valid_type] CHECK (Type in (''GeneralPicture'',''GeneralComment'',''PunchPicture'',''PunchComment'')),
        CONSTRAINT [FK_LabelHost_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_LabelHost_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[LabelHostHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231120123136_Labels'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [Labels] (
        [Id] int NOT NULL IDENTITY,
        [Text] nvarchar(60) NOT NULL,
        [IsVoided] bit NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_Labels] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_Labels_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_Labels_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[LabelsHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231120123136_Labels'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [LabelLabelHost] (
        [HostsId] int NOT NULL,
        [LabelsId] int NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        CONSTRAINT [PK_LabelLabelHost] PRIMARY KEY ([HostsId], [LabelsId]),
        CONSTRAINT [FK_LabelLabelHost_LabelHost_HostsId] FOREIGN KEY ([HostsId]) REFERENCES [LabelHost] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LabelLabelHost_Labels_LabelsId] FOREIGN KEY ([LabelsId]) REFERENCES [Labels] ([Id]) ON DELETE CASCADE,
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[LabelLabelHostHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231120123136_Labels'
)
BEGIN
    CREATE INDEX [IX_LabelHost_CreatedById] ON [LabelHost] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231120123136_Labels'
)
BEGIN
    CREATE INDEX [IX_LabelHost_ModifiedById] ON [LabelHost] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231120123136_Labels'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LabelHost_Type] ON [LabelHost] ([Type]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231120123136_Labels'
)
BEGIN
    CREATE INDEX [IX_LabelLabelHost_LabelsId] ON [LabelLabelHost] ([LabelsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231120123136_Labels'
)
BEGIN
    CREATE INDEX [IX_Labels_CreatedById] ON [Labels] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231120123136_Labels'
)
BEGIN
    CREATE INDEX [IX_Labels_ModifiedById] ON [Labels] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231120123136_Labels'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Labels_Text] ON [Labels] ([Text]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231120123136_Labels'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231120123136_Labels', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    ALTER TABLE [LabelHost] DROP CONSTRAINT [FK_LabelHost_Persons_CreatedById];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    ALTER TABLE [LabelHost] DROP CONSTRAINT [FK_LabelHost_Persons_ModifiedById];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    ALTER TABLE [LabelLabelHost] DROP CONSTRAINT [FK_LabelLabelHost_LabelHost_HostsId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    ALTER TABLE [LabelHost] SET (SYSTEM_VERSIONING = OFF)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    ALTER TABLE [LabelHost] DROP CONSTRAINT [PK_LabelHost];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    EXEC sp_rename N'[LabelHost]', N'LabelHosts';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    EXEC sp_rename N'[LabelHosts].[IX_LabelHost_Type]', N'IX_LabelHosts_Type', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    EXEC sp_rename N'[LabelHosts].[IX_LabelHost_ModifiedById]', N'IX_LabelHosts_ModifiedById', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    EXEC sp_rename N'[LabelHosts].[IX_LabelHost_CreatedById]', N'IX_LabelHosts_CreatedById', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    EXEC sp_rename N'[LabelHostHistory]', N'LabelHostsHistory';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    ALTER TABLE [OutboxMessage] ADD [MessageType] nvarchar(max) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    ALTER TABLE [LabelHosts] ADD CONSTRAINT [PK_LabelHosts] PRIMARY KEY ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    ALTER TABLE [LabelHosts] ADD CONSTRAINT [FK_LabelHosts_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    ALTER TABLE [LabelHosts] ADD CONSTRAINT [FK_LabelHosts_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    ALTER TABLE [LabelLabelHost] ADD CONSTRAINT [FK_LabelLabelHost_LabelHosts_HostsId] FOREIGN KEY ([HostsId]) REFERENCES [LabelHosts] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'ALTER TABLE [LabelHosts] SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + '].[LabelHostsHistory]))')
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231121131559_LabelHost'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231121131559_LabelHost', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231204094238_AttachmentLabels'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [AttachmentLabel] (
        [AttachmentId] int NOT NULL,
        [LabelsId] int NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        CONSTRAINT [PK_AttachmentLabel] PRIMARY KEY ([AttachmentId], [LabelsId]),
        CONSTRAINT [FK_AttachmentLabel_Attachments_AttachmentId] FOREIGN KEY ([AttachmentId]) REFERENCES [Attachments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_AttachmentLabel_Labels_LabelsId] FOREIGN KEY ([LabelsId]) REFERENCES [Labels] ([Id]) ON DELETE CASCADE,
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[AttachmentLabelHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231204094238_AttachmentLabels'
)
BEGIN
    CREATE INDEX [IX_AttachmentLabel_LabelsId] ON [AttachmentLabel] ([LabelsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231204094238_AttachmentLabels'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231204094238_AttachmentLabels', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231204120558_AttachmentDescription'
)
BEGIN
    ALTER TABLE [Attachments] ADD [Description] nvarchar(255) NOT NULL DEFAULT N'';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231204120558_AttachmentDescription'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231204120558_AttachmentDescription', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231206081348_CommentLabels'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [CommentLabel] (
        [CommentId] int NOT NULL,
        [LabelsId] int NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        CONSTRAINT [PK_CommentLabel] PRIMARY KEY ([CommentId], [LabelsId]),
        CONSTRAINT [FK_CommentLabel_Comments_CommentId] FOREIGN KEY ([CommentId]) REFERENCES [Comments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CommentLabel_Labels_LabelsId] FOREIGN KEY ([LabelsId]) REFERENCES [Labels] ([Id]) ON DELETE CASCADE,
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[CommentLabelHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231206081348_CommentLabels'
)
BEGIN
    CREATE INDEX [IX_CommentLabel_LabelsId] ON [CommentLabel] ([LabelsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231206081348_CommentLabels'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231206081348_CommentLabels', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    ALTER TABLE [LabelLabelHost] SET (SYSTEM_VERSIONING = OFF)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    DROP TABLE [LabelLabelHost];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    DROP TABLE [LabelLabelHostHistory];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    ALTER TABLE [LabelHosts] SET (SYSTEM_VERSIONING = OFF)
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    DROP TABLE [LabelHosts];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    DROP TABLE [LabelHostsHistory];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [LabelEntities] (
        [Id] int NOT NULL IDENTITY,
        [EntityWithLabel] nvarchar(450) NOT NULL DEFAULT N''PunchPicture'',
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_LabelEntities] PRIMARY KEY ([Id]),
        CONSTRAINT [valid_entity_type] CHECK (EntityWithLabel in (''PunchPicture'',''PunchComment'')),
        CONSTRAINT [FK_LabelEntities_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_LabelEntities_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[LabelEntitiesHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [LabelLabelEntity] (
        [EntitiesWithLabelId] int NOT NULL,
        [LabelsId] int NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        CONSTRAINT [PK_LabelLabelEntity] PRIMARY KEY ([EntitiesWithLabelId], [LabelsId]),
        CONSTRAINT [FK_LabelLabelEntity_LabelEntities_EntitiesWithLabelId] FOREIGN KEY ([EntitiesWithLabelId]) REFERENCES [LabelEntities] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_LabelLabelEntity_Labels_LabelsId] FOREIGN KEY ([LabelsId]) REFERENCES [Labels] ([Id]) ON DELETE CASCADE,
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[LabelLabelEntityHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    CREATE INDEX [IX_LabelEntities_CreatedById] ON [LabelEntities] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    CREATE UNIQUE INDEX [IX_LabelEntities_EntityWithLabel] ON [LabelEntities] ([EntityWithLabel]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    CREATE INDEX [IX_LabelEntities_ModifiedById] ON [LabelEntities] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    CREATE INDEX [IX_LabelLabelEntity_LabelsId] ON [LabelLabelEntity] ([LabelsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231207125745_RenameLabelHost'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231207125745_RenameLabelHost', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231211112710_RenameAvailableFor'
)
BEGIN
    ALTER TABLE [LabelLabelEntity] DROP CONSTRAINT [FK_LabelLabelEntity_LabelEntities_EntitiesWithLabelId];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231211112710_RenameAvailableFor'
)
BEGIN
    ALTER TABLE [LabelEntities] DROP CONSTRAINT [valid_entity_type];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231211112710_RenameAvailableFor'
)
BEGIN
    EXEC sp_rename N'[LabelLabelEntity].[EntitiesWithLabelId]', N'AvailableForId', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231211112710_RenameAvailableFor'
)
BEGIN
    EXEC sp_rename N'[LabelEntities].[EntityWithLabel]', N'EntityType', N'COLUMN';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231211112710_RenameAvailableFor'
)
BEGIN
    EXEC sp_rename N'[LabelEntities].[IX_LabelEntities_EntityWithLabel]', N'IX_LabelEntities_EntityType', N'INDEX';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231211112710_RenameAvailableFor'
)
BEGIN
    EXEC(N'ALTER TABLE [LabelEntities] ADD CONSTRAINT [valid_entity_type] CHECK (EntityType in (''PunchPicture'',''PunchComment''))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231211112710_RenameAvailableFor'
)
BEGIN
    ALTER TABLE [LabelLabelEntity] ADD CONSTRAINT [FK_LabelLabelEntity_LabelEntities_AvailableForId] FOREIGN KEY ([AvailableForId]) REFERENCES [LabelEntities] ([Id]) ON DELETE CASCADE;
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231211112710_RenameAvailableFor'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231211112710_RenameAvailableFor', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231219083729_Superuser'
)
BEGIN
    ALTER TABLE [Persons] ADD [Superuser] bit NOT NULL DEFAULT CAST(0 AS bit);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231219083729_Superuser'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231219083729_Superuser', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231219094424_Project_ProCoSy4LastUpdated'
)
BEGIN
    ALTER TABLE [Projects] ADD [ProCoSys4LastUpdated] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231219094424_Project_ProCoSy4LastUpdated'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231219094424_Project_ProCoSy4LastUpdated', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231221134230_CommentMentions'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [CommentPerson] (
        [CommentId] int NOT NULL,
        [MentionsId] int NOT NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        CONSTRAINT [PK_CommentPerson] PRIMARY KEY ([CommentId], [MentionsId]),
        CONSTRAINT [FK_CommentPerson_Comments_CommentId] FOREIGN KEY ([CommentId]) REFERENCES [Comments] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_CommentPerson_Persons_MentionsId] FOREIGN KEY ([MentionsId]) REFERENCES [Persons] ([Id]) ON DELETE CASCADE,
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[CommentPersonHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231221134230_CommentMentions'
)
BEGIN
    CREATE INDEX [IX_CommentPerson_MentionsId] ON [CommentPerson] ([MentionsId]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20231221134230_CommentMentions'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20231221134230_CommentMentions', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240103134733_Person_ProCoSys4LastUpdated'
)
BEGIN
    ALTER TABLE [Persons] ADD [ProCoSys4LastUpdated] datetime2 NOT NULL DEFAULT '0001-01-01T00:00:00.0000000';
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240103134733_Person_ProCoSys4LastUpdated'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240103134733_Person_ProCoSys4LastUpdated', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115070832_MailTemplate'
)
BEGIN
    DECLARE @historyTableSchema sysname = SCHEMA_NAME()
    EXEC(N'CREATE TABLE [MailTemplates] (
        [Id] int NOT NULL IDENTITY,
        [Code] nvarchar(64) NOT NULL,
        [Subject] nvarchar(512) NOT NULL,
        [Body] nvarchar(max) NOT NULL,
        [CreatedAtUtc] datetime2 NOT NULL,
        [CreatedById] int NOT NULL,
        [ModifiedAtUtc] datetime2 NULL,
        [ModifiedById] int NULL,
        [IsVoided] bit NOT NULL,
        [Plant] nvarchar(255) NULL,
        [PeriodEnd] datetime2 GENERATED ALWAYS AS ROW END HIDDEN NOT NULL,
        [PeriodStart] datetime2 GENERATED ALWAYS AS ROW START HIDDEN NOT NULL,
        [RowVersion] rowversion NULL,
        CONSTRAINT [PK_MailTemplates] PRIMARY KEY ([Id]),
        CONSTRAINT [FK_MailTemplates_Persons_CreatedById] FOREIGN KEY ([CreatedById]) REFERENCES [Persons] ([Id]),
        CONSTRAINT [FK_MailTemplates_Persons_ModifiedById] FOREIGN KEY ([ModifiedById]) REFERENCES [Persons] ([Id]),
        PERIOD FOR SYSTEM_TIME([PeriodStart], [PeriodEnd])
    ) WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = [' + @historyTableSchema + N'].[MailTemplatesHistory]))');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115070832_MailTemplate'
)
BEGIN
    EXEC(N'CREATE UNIQUE INDEX [IX_MailTemplates_Code] ON [MailTemplates] ([Code], [Plant]) INCLUDE ([Subject], [Body], [IsVoided]) WHERE [Plant] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115070832_MailTemplate'
)
BEGIN
    CREATE INDEX [IX_MailTemplates_CreatedById] ON [MailTemplates] ([CreatedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115070832_MailTemplate'
)
BEGIN
    CREATE INDEX [IX_MailTemplates_ModifiedById] ON [MailTemplates] ([ModifiedById]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115070832_MailTemplate'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240115070832_MailTemplate', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115071041_LibraryItemPlantConfiguration'
)
BEGIN
    DECLARE @var24 sysname;
    SELECT @var24 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[Library]') AND [c].[name] = N'Plant');
    IF @var24 IS NOT NULL EXEC(N'ALTER TABLE [Library] DROP CONSTRAINT [' + @var24 + '];');
    EXEC(N'UPDATE [Library] SET [Plant] = N'''' WHERE [Plant] IS NULL');
    ALTER TABLE [Library] ALTER COLUMN [Plant] nvarchar(255) NOT NULL;
    ALTER TABLE [Library] ADD DEFAULT N'' FOR [Plant];
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115071041_LibraryItemPlantConfiguration'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240115071041_LibraryItemPlantConfiguration', N'8.0.0');
END;
GO

COMMIT;
GO

BEGIN TRANSACTION;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115093210_MailTemplateBodyLengthMax'
)
BEGIN
    DROP INDEX [IX_MailTemplates_Code] ON [MailTemplates];
    DECLARE @var25 sysname;
    SELECT @var25 = [d].[name]
    FROM [sys].[default_constraints] [d]
    INNER JOIN [sys].[columns] [c] ON [d].[parent_column_id] = [c].[column_id] AND [d].[parent_object_id] = [c].[object_id]
    WHERE ([d].[parent_object_id] = OBJECT_ID(N'[MailTemplates]') AND [c].[name] = N'Body');
    IF @var25 IS NOT NULL EXEC(N'ALTER TABLE [MailTemplates] DROP CONSTRAINT [' + @var25 + '];');
    ALTER TABLE [MailTemplates] ALTER COLUMN [Body] nvarchar(2048) NOT NULL;
    EXEC(N'CREATE UNIQUE INDEX [IX_MailTemplates_Code] ON [MailTemplates] ([Code], [Plant]) INCLUDE ([Subject], [Body], [IsVoided]) WHERE [Plant] IS NOT NULL');
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20240115093210_MailTemplateBodyLengthMax'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20240115093210_MailTemplateBodyLengthMax', N'8.0.0');
END;
GO

COMMIT;
GO


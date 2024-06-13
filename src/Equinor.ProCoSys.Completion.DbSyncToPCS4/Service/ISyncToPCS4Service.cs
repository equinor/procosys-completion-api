namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.Service;

public interface ISyncToPCS4Service
{
    // PunchListItem
    Task SyncNewPunchListItemAsync(object addEvent, CancellationToken cancellationToken);
    Task SyncPunchListItemUpdateAsync(object updateEvent, CancellationToken cancellationToken);
    Task SyncPunchListItemDeleteAsync(object deleteEvent, CancellationToken cancellationToken);

    // Comment
    Task SyncNewCommentAsync(object addEvent, CancellationToken cancellationToken);

    // Attachment
    Task SyncNewAttachmentAsync(object addEvent, CancellationToken cancellationToken);
    Task SyncAttachmentUpdateAsync(object updateEvent, CancellationToken cancellationToken);
    Task SyncAttachmentDeleteAsync(object deleteEvent, CancellationToken cancellationToken);

    // Link (Attachment)
    Task SyncNewLinkAsync(object addEvent, CancellationToken cancellationToken);
    Task SyncLinkUpdateAsync(object updateEvent, CancellationToken cancellationToken);
    Task SyncLinkDeleteAsync(object deleteEvent, CancellationToken cancellationToken);
}

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4;

public class SyncToPCS4Constants
{
    // Sync Methods
    public const string Post = "POST";
    public const string Put = "PUT";
    public const string Delete = "DELETE";

    // Sync Object Names
    public const string PunchListItem = "PunchListItem";
    public const string Comment = "Comment";
    public const string Attachment = "Attachment";
    public const string Link = "Link";

    // URL endpoints
    public const string PunchListItemInsertEndpoint = "PunchListItem/Add";
    public const string PunchListItemUpdateEndpoint = "PunchListItem/Update";
    public const string PunchListItemDeleteEndpoint = "PunchListItem/Delete";

    public const string CommentInsertEndpoint = "PunchListItem/Comment/Add";

    public const string AttachmentInsertEndpoint = "Attachment/Add";
    public const string AttachmentUpdateEndpoint = "Attachment/Update";
    public const string AttachmentDeleteEndpoint = "Attachment/Delete";

    public const string LinkInsertEndpoint = "Link/Attachment/Add";
    public const string LinkUpdateEndpoint = "Link/Attachment/Update";
    public const string LinkDeleteEndpoint = "Link/Attachment/Delete";
}

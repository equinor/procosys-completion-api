namespace Equinor.ProCoSys.Completion.Command.MessageProducers;
public static class QueueNames
{
    public const string ClassificationCompletionTransferQueue = "classificationCompletionTransferQueue";
    public static string CompletionHistoryCreated = "completionHistoryCreatedQueue";
    public static string CompletionHistoryUpdated = "completionHistoryUpdatedQueue";
    public static string CompletionHistoryDeleted = "completionHistoryDeletedQueue";
    public static string LibraryCompletionTransferQueue = "libraryCompletionTransferQueue";
    public static string SwcrCompletionTransferQueue = "swcrCompletionTransferQueue";
    public static string DocumentCompletionTransferQueue = "documentCompletionTransferQueue";
    public static string WorkOrderCompletionTransferQueue = "workOrderCompletionTransferQueue";
    public static string PunchItemCompletionTransferQueue = "punchItemCompletionTransferQueue";
    public static string PunchItemChangeHistoryCompletionTransferQueue = "punchItemChangeHistoryCompletionTransferQueue";
    public static string ProjectCompletionTransferQueue = "projectCompletionTransferQueue";
    public static string PersonCompletionTransferQueue = "personCompletionTransferQueue";
    public static string PunchItemAttachmentCompletionTransferQueue = "punchItemAttachmentCompletionTransferQueue";
    public static string PunchItemCommentCompletionTransferQueue = "punchItemCommentCompletionTransferQueue";
}

namespace Equinor.ProCoSys.Completion.TieImport;

public static class PunchObjectAttributes
{
    public const string Class = "CLASS";
    public const string ClearedBy = "CLEAREDBY";
    public const string ClearedByOrganization = "CLEAREDBYORGANIZATION";
    public const string ClearedDate = "CLEAREDDATE";
    public const string Description = "DESCRIPTION";
    public const string DueDate = "DUEDATE";
    public const string ExternalPunchItemNo = "EXTERNALPUNCHITEMNO";
    public const string FormType = "FORMTYPE";
    public const string MaterialEta = "MATERIALETA";
    public const string MaterialNo = "MATERIALNO";
    public const string MaterialRequired = "MATERIALREQUIRED";
    public const string MethodVerb = "METHOD_VERB";
    public const string Project = "PROJECT";
    public const string PunchItemNo = "PUNCHITEMNO";
    public const string PunchListType = "PUNCHLISTTYPE";
    public const string RaisedByOrganization = "RAISEDBYORGANIZATION";
    public const string RejectedBy = "REJECTEDBY";
    public const string RejectedDate = "REJECTEDDATE";
    public const string Responsible = "RESPONSIBLE";
    public const string Status = "STATUS";
    public const string TagNo = "TAGNO";
    public const string VerifiedBy = "VERIFIEDBY";
    public const string VerifiedDate = "VERIFIEDDATE";
    
    public const string ActionBy = "ACTIONBYPERSON";
    public const string DocumentNo = "DOCUMENTNUMBER";
    public const string Estimate = "ESTIMATE";
    public const string OriginalWorkOrderNo = "ORIGINALWONUMBER";
    public const string WorkOrderNo = "WONUMBER";
    public const string Priority = "PRIORITY";
    public const string Sorting = "PUNCHLISTSORTING";
    public const string SwcrNo = "SWCRNUMBER";

    public const string IsVoided = "ISVOIDED";

    /// <summary>
    /// Special marker value indicating that a field should be cleared/set to null.
    /// </summary>
    public const string NullMarker = "{NULL}";

    public static class Methods
    {
        public const string Create = "CREATE";
        public const string Update = "UPDATE";
        public const string Append = "APPEND";
        public const string Delete = "DELETE";
    }
}

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
public class PcsPunchItemIn : PcsaObjectIn
{
    private string _tagNo;
    private string _clearedBy;
    private string _verifiedBy;
    private string _rejectedBy;

    //For identifying the PunchItem  combination Plant->Project->McPkgNo->TagNo->Responsible->FormType,(External)PunchItemNo
    public override string Name
    {
        get => ExternalPunchItemNo;
        set => ExternalPunchItemNo = value;
    }

    //public string McPkgNo { get; set; }   //The McPkgNo is unique within a Project
    public string Project { get; set; } //Name of the project it belongs to

    public string TagNo
    {
        get => _tagNo;
        set => _tagNo = value?.ToUpperInvariant().Trim();
    } //The tag this punch item is for

    public string PlantId { get; set; }

    //TagCheck data (may be more than one per tag, Responsible identifies the check)
    public string Responsible { get; set; } //Responsible; "Company", Some fab. site etc. Via TagCheck table, Relates to the Responsible table

    public string FormType { get; set; }    //Its form data

    public long? PunchItemNo { get; set; }

    //If more than one for the tag for a particular Responsible/FormType one of the below must be supplied
    //public string PunchItemNo { get; set; } //ProCoSys internal number (only meaningful for updates, not create. Since being assigned in the create process). Removed after grooming
    public string ExternalPunchItemNo { get; set; }  //External parties may use their own number for identification, unique for Responsible and Project

    public string PunchListType { get; set; } //library PUNCHLIST_TYPE

    public string Priority { get; set; } // library COMM_PRIORITY (Also used for punchitem)

    public string Description { get; set; }

    public string Status { get; set; } //library COMPLETION_STATUS (OK,PA,PB,OS)

    public string RaisedByOrganization { get; set; } //library COMPLETION_ORGANIZATION

    public string ClearedByOrganization { get; set; } //library COMPLETION_ORGANIZATION

    public DateTime? DueDate { get; set; }

    public DateTime? ClearedDate { get; set; }

    public bool MaterialRequired { get; set; }

    public DateTime? MaterialEta { get; set; }

    public string MaterialNo { get; set; }

    public string ClearedBy
    {
        get => _clearedBy;
        set => _clearedBy = value?.ToUpperInvariant().Trim();
    } //person (user in procosys)

    public DateTime? VerifiedDate { get; set; }

    public string VerifiedBy
    {
        get => _verifiedBy;
        set => _verifiedBy = value?.ToUpperInvariant().Trim();
    } //person (user in procosys)

    public DateTime? RejectedDate { get; set; }

    public string RejectedBy
    {
        get => _rejectedBy;
        set => _rejectedBy = value?.ToUpperInvariant().Trim();
    } //person (user in procosys)
}

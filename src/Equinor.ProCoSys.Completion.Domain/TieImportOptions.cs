namespace Equinor.ProCoSys.Completion.Domain;

public sealed class TieImportOptions
{
    public const string UserName = "PROCOSYS_IMPORT";
    public string ImportUserName { get; set; } = UserName;
}

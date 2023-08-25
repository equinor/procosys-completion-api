namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
public interface IPcsObjectIn
{
    //Declares all common import properties that any PcsImport class must implement
    string Name { get; set; }

    ImportMethod ImportMethod { get; set; }

    ImportOptions ImportOptions { get; set; }
}

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
public interface IPcsObjectIn
{
    //Declares all common import properties that any PcsImport class must implement
    string Name { get; set; }

    //TODO: 109740 ImportOptions ImportOptions { get; set; }
}

namespace Equinor.ProCoSys.Completion.TieImport.References;

public interface ICommandReferencesServiceFactory
{
    ICommandReferencesService Create(ImportDataBundle bundle);
}

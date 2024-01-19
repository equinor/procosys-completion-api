namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
public abstract class PcsaObjectBase : IPcsObject
{
    public abstract string Name { get; set; } //The various Pcs classes should override this one

    //TODO: 109740 ImportOptions
}

namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
public abstract class PcsaObjectIn : PcsaObjectBase, IPcsObjectIn
{
    public ImportMethod ImportMethod { get; set; }
}

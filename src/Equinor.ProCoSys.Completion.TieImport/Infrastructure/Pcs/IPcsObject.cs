namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
public interface IPcsObject
{
    //Declares all common properties that any Pcs communication class must implement
    //All objects must have a name, but property may be named as something else in ProCoSys, so may be implemented as an alias
    string Name { get; set; }
}

﻿namespace Equinor.ProCoSys.Completion.TieImport.Infrastructure.Pcs;
public abstract class PcsaObjectBase : IPcsObject
{
    public abstract string Name { get; set; } //The various Pcs classes should override this one

    public ImportOptions ImportOptions { get; set; }

    protected PcsaObjectBase() => ImportOptions = new ImportOptions();
}
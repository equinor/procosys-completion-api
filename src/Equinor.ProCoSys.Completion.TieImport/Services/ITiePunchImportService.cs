using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Services;

public interface ITiePunchImportService
{
    /// <summary>
    /// Imports a mapped TIObject of type PUNCHITEM to ProCoSys
    /// </summary>
    /// <param name="tiObject">TiObject, must have class = 'PUNCHITEM'</param>
    /// <returns></returns>
    Task<TIMessageResult> ImportMessage(TIObject tiObject);
}

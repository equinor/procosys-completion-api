//using Statoil.TI.InterfaceServices.Message;

//namespace Equinor.ProCoSys.Completion.TieImport.Extensions;
//public static class TIInterfaceMessageExtensions
//{
//    public static bool IsClass(this TIInterfaceMessage tieMessage, params string[] classes)
//        => tieMessage?.ObjectClass != null &&
//           classes.Any(c => tieMessage.ObjectClass.Equals(c, StringComparison.InvariantCultureIgnoreCase));

//    public static bool SourceIsStid(this TIInterfaceMessage tieMessage)
//        => tieMessage?.SourceSystem?.StartsWith("STID") ?? false;

//    public static bool TransferCodeIsStid(this TIInterfaceMessage tieMessage)
//        => tieMessage.TransferCode() == "STID";

//    public static bool SourceOrTransferCodeIsStid(this TIInterfaceMessage tieMessage)
//        => tieMessage.SourceIsStid() || tieMessage.TransferCodeIsStid();

//    public static string? TransferCode(this TIInterfaceMessage tieMessage)
//        => tieMessage?.GetMetadataValue("TRANSFER_CODE")?.ToUpper();

//    public static bool MetaDataAsBool(this TIInterfaceMessage tieMessage, string key)
//        => (tieMessage?.GetMetadataValue(key))!.GetValueAsBool();
//}

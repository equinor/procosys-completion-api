namespace Equinor.ProCoSys.Completion.MessageContracts.History;

// this enum describe both the data type of the value (int, bool, DateTime), and how it should
// be displayed to end users
public enum ValueDisplayType
{
    StringAsText,
    DateTimeAsDateAndTime,
    DateTimeAsDateOnly,
    BoolAsYesNo,
    BoolAsTrueFalse,
    IntAsText,
    // Serialized from IUser contract, containing Oid and FullName. Display just the FullName
    UserAsNameOnly,
    // Serialized from IUser contract, containing Oid and FullName. Display FullName as link to Equinor Entra
    UserAsNameAndPicture,
    // Serialized from IUser contract, containing Oid and FullName. Display FullName + (mini) picture from Equinor Entra
    UserAsLinkToAddressBook,
    // Serialized from IUser contract, containing Oid and FullName. Display FullName with Contact Card "popup"-function
    // showing user info from Equinor Entra
    UserAsContactCard
}

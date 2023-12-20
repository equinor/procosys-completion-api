namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.MappingConfig;

public class PunchItemMappingConfig : ISourceObjectMappingConfig
{
    public string TargetTable { get; } = "PunchListItem";

    public PropertyMapping PrimaryKey { get; } = new PropertyMapping("Guid", "Procosys_guid", PropertyType.Guid, null);

    public PunchItemMappingConfig() => PropertyMappings = new List<PropertyMapping>
        {
            new PropertyMapping("Description",         "Description",          PropertyType.String,   null),
            new PropertyMapping("RaisedByOrgGuid",     "RaisedByOrg_id",       PropertyType.Guid,     ValueConversion.GuidToLibId),
            new PropertyMapping("ClearingByOrgGuid",   "ClearedByOrg_id",      PropertyType.Guid,     ValueConversion.GuidToLibId),
            new PropertyMapping("ActionBy.Oid",        "ActionByPerson_id",    PropertyType.Guid,     ValueConversion.OidToPersonId),
            new PropertyMapping("DueTimeUtc",          "DueDate",              PropertyType.DateTime, null),         
            new PropertyMapping("Estimate",            "Estimate",             PropertyType.Int,      null),
            new PropertyMapping("PriorityGuid",        "Priority_id",          PropertyType.Guid,     ValueConversion.GuidToLibId),
            new PropertyMapping("SortingGuid",         "PunchListSorting_id",  PropertyType.Guid,     ValueConversion.GuidToLibId),
            new PropertyMapping("TypeGuid",            "PunchListType_id",     PropertyType.Guid,     ValueConversion.GuidToLibId),
            new PropertyMapping("OriginalWorkOrderGuid", "OriginalWO_id",      PropertyType.Guid,     ValueConversion.GuidToWorkOrderId),
            new PropertyMapping("WorkOrderGuid",       "WO_id",                PropertyType.Guid,     ValueConversion.GuidToWorkOrderId),
            new PropertyMapping("SWCRGuid",            "SWCR_id",              PropertyType.Guid,     ValueConversion.GuidToSWCRId),
            new PropertyMapping("DocumentGuid",        "Drawing_id",           PropertyType.Guid,     ValueConversion.GuidToDocumentId),
            new PropertyMapping("ExternalItemNo",      "External_ItemNo",      PropertyType.String,   null),
            new PropertyMapping("MaterialRequired",    "IsMaterialRequired",   PropertyType.Bool,     null),
            new PropertyMapping("MaterialETAUtc",      "Material_ETA",         PropertyType.DateTime, null),
            new PropertyMapping("MaterialExternalNo",  "MaterialNo",           PropertyType.String,   null)
        };

    public List<PropertyMapping> PropertyMappings { get; }
}

namespace Equinor.ProCoSys.Completion.DbSyncToPCS4.MappingConfig;

public class PunchItemMappingConfig : ISourceObjectMappingConfig
{
    public string TargetTable { get; } = "PunchListItem";

    public PropertyMapping PrimaryKey { get; } = new PropertyMapping("Guid", PropertyType.Guid, "Procosys_guid", null, null, null);

    public PunchItemMappingConfig() => PropertyMappings = new List<PropertyMapping>
        {
            new PropertyMapping("Plant",                  PropertyType.String,      "ProjectSchema",            null,                                   null,                   true),
            new PropertyMapping("Id",                     PropertyType.Int,         "PunchListItem_id",         null,                                   "SEQ_PUNCHLISTITEM",    true),
            new PropertyMapping("CheckListGuid",          PropertyType.Guid,        "TagCheck_id",              ValueConversion.GuidToTagCheckId,       null,                   true),
            new PropertyMapping("CreatedBy.Oid",          PropertyType.Guid,        "CreatedBy_id",             ValueConversion.OidToPersonId,          null,                   true),
            new PropertyMapping("Category",               PropertyType.String,      "Status_id",                ValueConversion.PunchCategoryToLibId,   null,                   false),
            new PropertyMapping("Description",            PropertyType.String,      "Description",              null,                                   null,                   false),
            new PropertyMapping("RaisedByOrgGuid",        PropertyType.Guid,        "RaisedByOrg_id",           ValueConversion.GuidToLibId,            null,                   false),
            new PropertyMapping("ClearingByOrgGuid",      PropertyType.Guid,        "ClearedByOrg_id",          ValueConversion.GuidToLibId,            null,                   false),
            new PropertyMapping("ActionBy.Oid",           PropertyType.Guid,        "ActionByPerson_id",        ValueConversion.OidToPersonId,          null,                   false),
            new PropertyMapping("DueTimeUtc",             PropertyType.DateTime,    "DueDate",                  null,                                   null,                   false),
            new PropertyMapping("Estimate",               PropertyType.Int,         "Estimate",                 null,                                   null,                   false),
            new PropertyMapping("PriorityGuid",           PropertyType.Guid,        "Priority_id",              ValueConversion.GuidToLibId,            null,                   false),
            new PropertyMapping("SortingGuid",            PropertyType.Guid,        "PunchListSorting_id",      ValueConversion.GuidToLibId,            null,                   false),
            new PropertyMapping("TypeGuid",               PropertyType.Guid,        "PunchListType_id",         ValueConversion.GuidToLibId,            null,                   false),
            new PropertyMapping("OriginalWorkOrderGuid",   PropertyType.Guid,       "OriginalWO_id",            ValueConversion.GuidToWorkOrderId,      null,                   false),
            new PropertyMapping("WorkOrderGuid",          PropertyType.Guid,        "WO_id",                    ValueConversion.GuidToWorkOrderId,      null,                   false),
            new PropertyMapping("SWCRGuid",               PropertyType.Guid,        "SWCR_id",                  ValueConversion.GuidToSWCRId,           null,                   false),
            new PropertyMapping("DocumentGuid",           PropertyType.Guid,        "Drawing_id",               ValueConversion.GuidToDocumentId,       null,                   false),
            new PropertyMapping("ExternalItemNo",         PropertyType.String,      "External_ItemNo",          null,                                   null,                   false),
            new PropertyMapping("MaterialRequired",       PropertyType.Bool,        "IsMaterialRequired",       null,                                   null,                   false),
            new PropertyMapping("MaterialETAUtc",         PropertyType.DateTime,    "Material_ETA",             null,                                   null,                   false),
            new PropertyMapping("MaterialExternalNo",     PropertyType.String,      "MaterialNo",               null,                                   null,                   false)
        };

    public List<PropertyMapping> PropertyMappings { get; }
}

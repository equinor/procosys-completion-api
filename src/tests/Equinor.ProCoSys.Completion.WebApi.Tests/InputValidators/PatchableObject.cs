using System;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.InputValidators;

internal class PatchableObject
{
    public string RowVersion { get; set; } = string.Empty;
    public string MyString { get; set; } = string.Empty;
    public string? MyNullableString1 { get; set; }
    public string? MyNullableString2 { get; set; }
    public int MyInt { get; set; }
    public int? MyNullableInt1 { get; set; }
    public int? MyNullableInt2 { get; set; }
    public Guid MyGuid { get; set; } = Guid.Empty;
    public Guid? MyNullableGuid1 { get; set; }
    public Guid? MyNullableGuid2 { get; set; }
    public DateTime MyDateTime { get; set; }
    public DateTime? MyNullableDateTime1 { get; set; }
    public DateTime? MyNullableDateTime2 { get; set; }
}

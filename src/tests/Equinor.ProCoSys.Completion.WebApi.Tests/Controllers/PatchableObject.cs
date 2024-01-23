using System;
using System.ComponentModel.DataAnnotations;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Controllers;

internal class PatchableObject
{
    internal const int MyStringMinimumLength = 3;
    internal const int MyStringMaximumLength = 8;

    // need RequiredAttribute to distinguish between type of string and nullable string?
    // when using refection to check property type of string and string? both return "System.String"
    // the ideal has been to just check if type is nullable or not to determine if it's required
    [Required]
    public string RowVersion { get; set; } = string.Empty;
    [Required]
    [StringLength(MyStringMaximumLength, MinimumLength = MyStringMinimumLength)]
    public string MyString { get; set; } = string.Empty;
    public string? MyNullableString1 { get; set; }
    public string? MyNullableString2 { get; set; }
    [Required]
    public int MyInt { get; set; }
    public int? MyNullableInt1 { get; set; }
    public int? MyNullableInt2 { get; set; }
    [Required]
    public double MyDouble { get; set; }
    public double? MyNullableDouble1 { get; set; }
    public double? MyNullableDouble2 { get; set; }
    [Required]
    public Guid MyGuid { get; set; } = Guid.Empty;
    public Guid? MyNullableGuid1 { get; set; }
    public Guid? MyNullableGuid2 { get; set; }
    [Required]
    public DateTime MyDateTime { get; set; }
    public DateTime? MyNullableDateTime1 { get; set; }
    public DateTime? MyNullableDateTime2 { get; set; }
}

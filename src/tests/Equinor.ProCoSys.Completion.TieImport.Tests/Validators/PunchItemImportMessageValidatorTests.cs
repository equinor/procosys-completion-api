using Equinor.ProCoSys.Completion.TieImport.Validators;
using FluentValidation.TestHelper;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.TieImport.Models;

namespace Equinor.ProCoSys.Completion.TieImport.Tests.Validators;

[TestClass]
public sealed class PunchItemImportMessageValidatorTests
{
    private PunchItemImportMessageValidator _dut = null!;

    private readonly PunchItemImportMessage _baseMessage = new(
        Guid.NewGuid(), 
        "CREATE", 
        "Projectname", 
        "Plant", 
        "TagNo", 
        "ExternalPunchItemNo",
        "FormType",
        "EQ", 
        new Optional<long?>(), 
        new Optional<string?>(), 
        new Optional<string?>(), 
        Category.PA, 
        new OptionalWithNull<string?>(), 
        new OptionalWithNull<DateTime?>(), 
        new Optional<DateTime?>(),
        new Optional<string?>(), 
        new Optional<string?>(),
        new Optional<DateTime?>(), 
        new Optional<string?>(), 
        new Optional<DateTime?>(), 
        new Optional<string?>(),
        new Optional<bool?>(),
        new OptionalWithNull<DateTime?>(), 
        new OptionalWithNull<string?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<int?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<string?>(),
        new OptionalWithNull<int?>(),
        new Optional<string?>());

    [TestInitialize]
    public void Setup() =>
        _dut = new PunchItemImportMessageValidator();

    [TestMethod]
    public void ValidateOk()
    {
        var message = _baseMessage with { Description = new Optional<string?>("Test") };
        var result = _dut.TestValidate(message);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [TestMethod]
    public void ValidateCategoryIsRequired()
    {
        var message = _baseMessage with { Category = null };
        var result = _dut.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }

    [TestMethod]
    public void ValidateDescriptionIsRequiredForCreateMethod()
    {
        var result = _dut.TestValidate(_baseMessage);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [TestMethod]
    public void ValidateRejectedDateAndRejectedByMustBeSetTogether()
    {
        var message = _baseMessage with { RejectedDate = new Optional<DateTime?>(DateTime.Now) };
        var result = _dut.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.RejectedBy);

        message = _baseMessage with { RejectedBy = new Optional<string?>("SKS@equinor.com") };
        result = _dut.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.RejectedDate);
    }

    [TestMethod]
    public void ValidateVerifiedDateAndVerifiedByMustBeSetTogether()
    {
        var message = _baseMessage with { VerifiedDate = new Optional<DateTime?>(DateTime.Now) };
        var result = _dut.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.VerifiedBy);

        message = _baseMessage with { VerifiedBy = new Optional<string?>("SKS@equinor.com") };
        result = _dut.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.VerifiedDate);
    }

    [TestMethod]
    public void ValidateClearedDateAndClearedByMustBeSetTogether()
    {
        var message = _baseMessage with { ClearedDate = new Optional<DateTime?>(DateTime.Now) };
        var result = _dut.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.ClearedBy);

        message = _baseMessage with { ClearedBy = new Optional<string?>("SKS@equinor.com") };
        result = _dut.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.ClearedDate);
    }
}

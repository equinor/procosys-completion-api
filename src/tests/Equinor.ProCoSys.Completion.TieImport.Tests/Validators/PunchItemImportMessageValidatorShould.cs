using Equinor.ProCoSys.Completion.Domain.Imports;
using Equinor.ProCoSys.Completion.TieImport.Validators;
using FluentValidation.TestHelper;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;

namespace Equinor.ProCoSys.Completion.TieImport.Tests.Validators;

[TestClass]
public sealed class PunchItemImportMessageValidatorShould
{
    private PunchItemImportMessageValidator _validator;

    [TestInitialize]
    public void Setup()
    {
        _validator = new PunchItemImportMessageValidator();
    }

    [TestMethod]
    public void ValidateCategoryIsRequired()
    {
        var message = new PunchItemImportMessage(
            Guid.NewGuid(), "Plant", "Method", "ProjectName", "TagNo", "ExternalPunchItemNo", "FormType",
            new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
            null, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>(), new Optional<DateTime?>(), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>()
        );
        var result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }

    [TestMethod]
    public void ValidateDescriptionIsRequiredForCreateMethod()
    {
        var message = new PunchItemImportMessage(
            Guid.NewGuid(), "Plant", PunchObjectAttributes.Methods.Create, "ProjectName", "TagNo", "ExternalPunchItemNo", "FormType",
            new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
            Category.PA, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>(), new Optional<DateTime?>(), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>()
        );
        var result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [TestMethod]
    public void ValidateRejectedDateAndRejectedByMustBeSetTogether()
    {
        var message = new PunchItemImportMessage(
            Guid.NewGuid(), "Plant", "Method", "ProjectName", "TagNo", "ExternalPunchItemNo", "FormType",
            new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
            Category.PA, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(DateTime.Now), new Optional<string?>(), new Optional<DateTime?>(DateTime.Now), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>()
        );
        var result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.RejectedBy);

        message = new PunchItemImportMessage(
            Guid.NewGuid(), "Plant", "Method", "ProjectName", "TagNo", "ExternalPunchItemNo", "FormType",
            new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
            Category.PA, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>(), new Optional<DateTime?>(), new Optional<string?>("SKS@equinor.com"), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>()
        );
        result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.RejectedDate);
    }

    [TestMethod]
    public void ValidateVerifiedDateAndVerifiedByMustBeSetTogether()
    {
        var message = new PunchItemImportMessage(
            Guid.NewGuid(), "Plant", "Method", "ProjectName", "TagNo", "ExternalPunchItemNo", "FormType",
            new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
            Category.PA, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(DateTime.Now), new Optional<string?>(), new Optional<DateTime?>(DateTime.Now), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>()
        );
        var result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.VerifiedBy);

        message = new PunchItemImportMessage(
            Guid.NewGuid(), "Plant", "Method", "ProjectName", "TagNo", "ExternalPunchItemNo", "FormType",
            new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
            Category.PA, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>("SKS@equinor.com"), new Optional<DateTime?>(), new Optional<string?>("User"), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>()
        );
        result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.VerifiedDate);
    }

    [TestMethod]
    public void ValidateClearedDateAndClearedByMustBeSetTogether()
    {
        var message = new PunchItemImportMessage(
            Guid.NewGuid(), "Plant", "Method", "ProjectName", "TagNo", "ExternalPunchItemNo", "FormType",
            new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
            Category.PA, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(DateTime.Now), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>(), new Optional<DateTime?>(), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>()
        );
        var result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.ClearedBy);

        message = new PunchItemImportMessage(
            Guid.NewGuid(), "Plant", "Method", "ProjectName", "TagNo", "ExternalPunchItemNo", "FormType",
            new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
            Category.PA, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(), new Optional<string?>("SKS@equinor.com"), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>(), new Optional<DateTime?>(), new Optional<string?>(), new Optional<string?>(),
            new Optional<DateTime?>(), new Optional<string?>()
        );
        result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.ClearedDate);
    }
}

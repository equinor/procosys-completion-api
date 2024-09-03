using Equinor.ProCoSys.Completion.TieImport.Validators;
using FluentValidation.TestHelper;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Statoil.TI.InterfaceServices.Message;

namespace Equinor.ProCoSys.Completion.TieImport.Tests.Validators;

[TestClass]
public sealed class PunchItemImportMessageValidatorShould
{
    private PunchItemImportMessageValidator _validator = null!;

    private readonly PunchItemImportMessage _baseMessage = new(
        new TIObject{Guid = Guid.NewGuid(), Site = "Plant", Method = "CREATE", Project = "ProjectName"},  "TagNo", "ExternalPunchItemNo",
        "FormType",
        "EQ", new Optional<string?>(), new Optional<string?>(), new Optional<string?>(), new Optional<string?>(),
        Category.PA, new Optional<string?>(), new Optional<DateTime?>(), new Optional<DateTime?>(),
        new Optional<string?>(), new Optional<string?>(),
        new Optional<DateTime?>(), new Optional<string?>(), new Optional<DateTime?>(), new Optional<string?>(),
        new Optional<string?>(),
        new Optional<DateTime?>(), new Optional<string?>());

    [TestInitialize]
    public void Setup() =>
        _validator = new PunchItemImportMessageValidator(new PlantScopedImportDataContext("TestPlant"));

    [TestMethod]
    public void ValidateCategoryIsRequired()
    {
        var message = _baseMessage with { Category = null };
        var result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }

    [TestMethod]
    public void ValidateDescriptionIsRequiredForCreateMethod()
    {
        var result = _validator.TestValidate(_baseMessage);
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [TestMethod]
    public void ValidateRejectedDateAndRejectedByMustBeSetTogether()
    {
        var message = _baseMessage with { RejectedDate = new Optional<DateTime?>(DateTime.Now) };
        var result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.RejectedBy);

        message = _baseMessage with { RejectedBy = new Optional<string?>("SKS@equinor.com") };
        result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.RejectedDate);
    }

    [TestMethod]
    public void ValidateVerifiedDateAndVerifiedByMustBeSetTogether()
    {
        var message = _baseMessage with { VerifiedDate = new Optional<DateTime?>(DateTime.Now) };
        var result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.VerifiedBy);

        message = _baseMessage with { VerifiedBy = new Optional<string?>("SKS@equinor.com") };
        result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.VerifiedDate);
    }

    [TestMethod]
    public void ValidateClearedDateAndClearedByMustBeSetTogether()
    {
        var message = _baseMessage with { ClearedDate = new Optional<DateTime?>(DateTime.Now) };
        var result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.ClearedBy);

        message = _baseMessage with { ClearedBy = new Optional<string?>("SKS@equinor.com") };
        result = _validator.TestValidate(message);
        result.ShouldHaveValidationErrorFor(x => x.ClearedDate);
    }
}

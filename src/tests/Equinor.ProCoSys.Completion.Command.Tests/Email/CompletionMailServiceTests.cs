using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.TemplateTransforming;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Command.Tests.Email;

[TestClass]
public class CompletionMailServiceTests
{
    private CompletionMailService _dut;
    private IPlantProvider _plantProviderMock;
    private IPersonRepository _personRepositoryMock;
    private IMailTemplateRepository _mailTemplateRepositoryMock;
    private ITemplateTransformer _templateTransformerMock;
    private IEmailService _emailServiceMock;
    private IOptionsMonitor<ApplicationOptions> _optionsMock;
    
    private readonly object _context = new Whatever();
    private readonly string _code = "A";
    private readonly List<string> _eMailAddresses = ["a@pcs.no", "b@pcs.no"];
    private readonly string _plant = "P";
    private readonly string _transformedSubject = "S";
    private readonly string _transformedBody = "B";

    [TestInitialize]
    public void Setup()
    {
        _optionsMock = Substitute.For<IOptionsMonitor<ApplicationOptions>>();
        _optionsMock.CurrentValue.Returns(new ApplicationOptions { FakeEmail = false });
        _plantProviderMock = Substitute.For<IPlantProvider>();
        _plantProviderMock.Plant.Returns(_plant);

        _mailTemplateRepositoryMock = Substitute.For<IMailTemplateRepository>();
        var mailTemplate = new MailTemplate(_code, "S", "B");
        _mailTemplateRepositoryMock.GetByCodeAsync(_plant, _code, Arg.Any<CancellationToken>())
            .Returns(mailTemplate);

        _templateTransformerMock = Substitute.For<ITemplateTransformer>();
        _templateTransformerMock.Transform(mailTemplate.Subject, _context)
            .Returns(_transformedSubject);
        _templateTransformerMock.Transform(mailTemplate.Body, _context)
            .Returns(_transformedBody);

        _personRepositoryMock = Substitute.For<IPersonRepository>();
        _emailServiceMock = Substitute.For<IEmailService>();

        _dut = new CompletionMailService(
            _plantProviderMock,
            _personRepositoryMock,
            _mailTemplateRepositoryMock,
            _templateTransformerMock,
            _emailServiceMock,
            Substitute.For<ILogger<CompletionMailService>>(),
            _optionsMock);
    }

    [TestMethod]
    public async Task SendEmailAsync_ShouldSendTransformedMailToMailAddresses()
    {
        // Act
        await _dut.SendEmailAsync(_context, _code, _eMailAddresses, CancellationToken.None);

        // Assert
        await _emailServiceMock.Received(1).SendEmailsAsync(
            _eMailAddresses, 
            _transformedSubject, 
            _transformedBody, 
            Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task SendEmailAsync_ShouldSendTransformedMailToCurrentUserEmail_WhenFakeEmailEnabled()
    {
        // Arrange
        List<string> emailSentTo = null;
        string sentEmailSubject = null;
        string sentEmailBody = null;
        _emailServiceMock
            .When(x => x.SendEmailsAsync(
                Arg.Any<List<string>>(),
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo =>
            {
                emailSentTo = callInfo.ArgAt<List<string>>(0);
                sentEmailSubject = callInfo.ArgAt<string>(1);
                sentEmailBody = callInfo.ArgAt<string>(2);
            });
        _optionsMock.CurrentValue.Returns(new ApplicationOptions { FakeEmail = true });
        var currentPerson = new Person(Guid.NewGuid(), null!, null!, null!, "current@pcs.no", false);
        _personRepositoryMock.GetCurrentPersonAsync(CancellationToken.None)
            .Returns(currentPerson);
        
        // Act
        await _dut.SendEmailAsync(_context, _code, _eMailAddresses, CancellationToken.None);

        // Assert
        await _emailServiceMock.Received(1).SendEmailsAsync(
            Arg.Any<List<string>>(),
            Arg.Any<string>(),
            Arg.Any<string>(),
            Arg.Any<CancellationToken>());
        Assert.AreEqual(1, emailSentTo.Count);
        Assert.AreEqual(currentPerson.Email, emailSentTo.ElementAt(0));

        Assert.IsTrue(sentEmailSubject.StartsWith(_transformedSubject));
        Assert.IsTrue(sentEmailSubject.Contains("fake email"));
        Assert.IsTrue(sentEmailBody.EndsWith(_transformedBody));
        Assert.IsTrue(sentEmailBody.Contains("fake email"));
    }
}

internal class Whatever;

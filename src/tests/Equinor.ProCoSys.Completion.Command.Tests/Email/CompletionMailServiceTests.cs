using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Misc;
using Equinor.ProCoSys.Common.TemplateTransforming;
using Equinor.ProCoSys.Completion.Command.Email;
using Equinor.ProCoSys.Completion.Command.MessageProducers;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.MailEvents;
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
    private IMessageProducer _messageProducerMock;
    private IOptionsMonitor<ApplicationOptions> _optionsMock;
    
    private readonly object _context = new Whatever();
    private readonly string _code = "A";
    private readonly List<string> _emailAddresses = ["a@pcs.no", "b@pcs.no"];
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
        _mailTemplateRepositoryMock.GetNonVoidedByCodeAsync(_plant, _code, Arg.Any<CancellationToken>())
            .Returns(mailTemplate);

        _templateTransformerMock = Substitute.For<ITemplateTransformer>();
        _templateTransformerMock.Transform(mailTemplate.Subject, _context)
            .Returns(_transformedSubject);
        _templateTransformerMock.Transform(mailTemplate.Body, _context)
            .Returns(_transformedBody);

        _personRepositoryMock = Substitute.For<IPersonRepository>();
        _messageProducerMock = Substitute.For<IMessageProducer>();

        _dut = new CompletionMailService(
            _plantProviderMock,
            _personRepositoryMock,
            _mailTemplateRepositoryMock,
            _templateTransformerMock,
            _messageProducerMock,
            Substitute.For<ILogger<CompletionMailService>>(),
            _optionsMock);
    }

    [TestMethod]
    public async Task SendEmailEventAsync_ShouldNotSendAnyMailEvent_WhenNoEmailAddresses()
    {
        // Act
        await _dut.SendEmailEventAsync(_code, _context, [], CancellationToken.None);

        // Assert
        await _messageProducerMock.Received(0).SendEmailEventAsync(
            Arg.Any<SendEmailEvent>(),
            Arg.Any<CancellationToken>());
    }

    [TestMethod]
    public async Task SendEmailEventAsync_ShouldSendTransformedMailEventToMailAddresses()
    {
        // Arrange
        SendEmailEvent sendEmailEvent = null!;
        _messageProducerMock
            .When(x => x.SendEmailEventAsync(
                Arg.Any<SendEmailEvent>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo => sendEmailEvent = callInfo.Arg<SendEmailEvent>());
        
        // Act
        await _dut.SendEmailEventAsync(_code, _context, _emailAddresses, CancellationToken.None);

        // Assert
        await _messageProducerMock.Received(1).SendEmailEventAsync(
            Arg.Any<SendEmailEvent>(),
            Arg.Any<CancellationToken>());
        Assert.IsNotNull(sendEmailEvent);
        CollectionAssert.AreEqual(_emailAddresses, sendEmailEvent.To);
        Assert.AreEqual(_transformedSubject, sendEmailEvent.Subject);
        Assert.AreEqual(_transformedBody, sendEmailEvent.Body);
    }

    [TestMethod]
    public async Task SendEmailEventAsync_ShouldSendTransformedMailEventToCurrentUserEmail_WhenFakeEmailEnabled()
    {
        // Arrange
        SendEmailEvent sendEmailEvent = null!;
        _messageProducerMock
            .When(x => x.SendEmailEventAsync(
                Arg.Any<SendEmailEvent>(),
                Arg.Any<CancellationToken>()))
            .Do(callInfo => sendEmailEvent = callInfo.Arg<SendEmailEvent>());
        _optionsMock.CurrentValue.Returns(new ApplicationOptions { FakeEmail = true });
        var currentPerson = new Person(Guid.NewGuid(), null!, null!, null!, "current@pcs.no", false);
        _personRepositoryMock.GetCurrentPersonAsync(CancellationToken.None)
            .Returns(currentPerson);
        
        // Act
        await _dut.SendEmailEventAsync(_code, _context, _emailAddresses, CancellationToken.None);

        // Assert
        await _messageProducerMock.Received(1).SendEmailEventAsync(
            Arg.Any<SendEmailEvent>(),
            Arg.Any<CancellationToken>());
        Assert.IsNotNull(sendEmailEvent);

        Assert.AreEqual(1, sendEmailEvent.To.Count);
        Assert.AreEqual(currentPerson.Email, sendEmailEvent.To.ElementAt(0));

        Assert.IsTrue(sendEmailEvent.Subject.StartsWith(_transformedSubject));
        Assert.IsTrue(sendEmailEvent.Subject.Contains("fake email"));
        Assert.IsTrue(sendEmailEvent.Body.EndsWith(_transformedBody));
        Assert.IsTrue(sendEmailEvent.Body.Contains("fake email"));
    }
}

internal class Whatever;

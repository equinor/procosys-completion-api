using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Common.Email;
using Equinor.ProCoSys.Completion.Domain.Events.IntegrationEvents.MailEvents;
using Equinor.ProCoSys.Completion.WebApi.Synchronization;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.WebApi.Tests.Synchronization;

[TestClass]
public class SendEmailEventConsumerTests
{
    private readonly ConsumeContext<SendEmailEvent> _contextMock = Substitute.For<ConsumeContext<SendEmailEvent>>();
    private readonly IEmailService _emailServiceMock = Substitute.For<IEmailService>();
    private SendEmailEventConsumer _dut = null!;

    [TestMethod]
    public async Task Consume_Should_SendEmail()
    {
        //Arrange
        _dut = new SendEmailEventConsumer(Substitute.For<ILogger<SendEmailEventConsumer>>(), _emailServiceMock);
        var bEvent = new SendEmailEvent(["a1@b.com", "a2@b.com"], "S", "B");
        _contextMock.Message.Returns(bEvent);

        //Act
        await _dut.Consume(_contextMock);

        //Assert
        await _emailServiceMock.Received(1)
            .SendEmailsAsync(bEvent.To, bEvent.Subject, bEvent.Body, Arg.Any<CancellationToken>());
    }
}

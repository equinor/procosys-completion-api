using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Email;

public interface ICompletionMailService
{
    /*
        Sample of how to build dynamic content to be used to replace placeholders in an email template

        string status = "Your order is sent";
        MyPerson person = new MyPerson { Name = "John Doe" };
        MyOrder order = new MyOrder {
            Person = person,
            Id = 101
        }

        dynamic context = new ExpandoObject();
        context.Order = order;
        context.Status = status;

        Sample email template with 3 placeholders:
        string mailTemplate = "Hi {{MyClass.Person.Name}}, status of your order {{Order.Id}} is '{{Status}}'."
    
        ITemplateTransformer.Transform(mailTemplate, context) will return:
        "Hi John Doe, status of your order 101 is 'Your order is sent'."
    */

    /// <summary>
    /// Create a SendEmailEvent and use MassTransit the event to the service bus. The actual email is sent in consumer of the SendEmailEvent
    /// </summary>
    /// <param name="templateCode">Code to a unique email template with placeholders in MailTemplate table</param>
    /// <param name="emailContext">A dynamic context, used to replace placeholders with dynamic data</param>
    /// <param name="emailAddresses">List of email addresses to send email to</param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task SendEmailEventAsync(
        string templateCode,
        dynamic emailContext,
        List<string> emailAddresses, 
        CancellationToken cancellationToken);
}

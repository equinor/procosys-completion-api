using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Equinor.ProCoSys.Completion.Command.Email;

public interface ICompletionMailService
{
    /*
        Sample of how to build dynamic content to be used to replace placeholders in a mail template

        string status = "Your order is sent";
        MyPerson person = new MyPerson { Name = "John Doe" };
        MyOrder order = new MyOrder {
            Person = person,
            Id = 101
        }

        dynamic context = new ExpandoObject();
        context.Order = order;
        context.Status = status;

        Sample mail template with 3 placeholders:
        string mailTemplate = "Hi {{MyClass.Person.Name}}, status of your order {{Order.Id}} is '{{Status}}'."
    
        ITemplateTransformer.Transform(mailTemplate, context) will return:
        "Hi John Doe, status of your order 101 is 'Your order is sent'."
    */
    Task SendEmailAsync(
        string templateCode,
        dynamic emailContext,
        List<string> emailAddresses, 
        CancellationToken cancellationToken);
}

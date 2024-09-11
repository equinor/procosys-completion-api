using System.Collections.Generic;
using MediatR;

namespace Equinor.ProCoSys.Completion.Query.MailTemplateQueries.GetAllMailTemplates;

public class GetAllMailTemplatesQuery : IRequest<IEnumerable<MailTemplateDto>>;

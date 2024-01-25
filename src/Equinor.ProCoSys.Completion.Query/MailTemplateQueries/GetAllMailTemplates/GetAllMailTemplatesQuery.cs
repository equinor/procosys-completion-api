using System.Collections.Generic;
using MediatR;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Query.MailTemplateQueries.GetAllMailTemplates;

public class GetAllMailTemplatesQuery : IRequest<Result<IEnumerable<MailTemplateDto>>>;

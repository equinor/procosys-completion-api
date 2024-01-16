using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.MailTemplateAggregate;
using Equinor.ProCoSys.Completion.Infrastructure.Repositories;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MockQueryable.NSubstitute;
using NSubstitute;

namespace Equinor.ProCoSys.Completion.Infrastructure.Tests.Repositories;

[TestClass]
public class MailTemplateRepositoryTests : RepositoryTestBase<MailTemplate>
{
    protected override EntityRepository<MailTemplate> GetDut()
        => new MailTemplateRepository(_contextHelper.ContextMock);

    protected override void SetupRepositoryWithOneKnownItem()
    {
        var mailTemplate = new MailTemplate("c", "s", "b");
        mailTemplate.SetProtectedIdForTesting(_knownId);

        var mailTemplates = new List<MailTemplate> { mailTemplate };

        _dbSetMock = mailTemplates.AsQueryable().BuildMockDbSet();

        _contextHelper
            .ContextMock
            .MailTemplates
            .Returns(_dbSetMock);
    }

    protected override MailTemplate GetNewEntity() => new("c", "s", "b");
}

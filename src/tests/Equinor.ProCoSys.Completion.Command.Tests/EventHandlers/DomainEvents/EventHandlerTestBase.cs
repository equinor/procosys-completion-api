using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Common.Time;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.AttachmentAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LabelAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.LinkAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PunchItemAggregate;
using Equinor.ProCoSys.Completion.Test.Common;
using Equinor.ProCoSys.Completion.Test.Common.ExtensionMethods;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers.DomainEvents;

public class EventHandlerTestBase
{
    protected DateTime _now = new(2021, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    protected Person _person;
    protected Link _link;
    protected Attachment _attachment;

    [TestInitialize]
    public void SetupBase()
    {
        TimeService.SetProvider(new ManualTimeProvider(_now));
        _person = new Person(Guid.NewGuid(), "Yo", "Da", "YD", "@", false);
        _person.SetProtectedIdForTesting(3);

        _link = new Link(nameof(PunchItem), Guid.NewGuid(), "A", "u");
        _attachment = new Attachment(nameof(PunchItem), Guid.NewGuid(), "PCS$PLANT", "file.txt");
        _attachment.UpdateLabels(new List<Label>{new("A"), new("B")});
    }

    protected void AssertSameLabels(List<Label> labelList1, List<string> labelList2)
    {
        Assert.AreEqual(labelList1.Count, labelList2.Count);
        for (var i = 0; i < labelList1.Count; i++)
        {
            Assert.AreEqual(labelList1.ElementAt(i).Text, labelList2.ElementAt(i));
        }
    }
}

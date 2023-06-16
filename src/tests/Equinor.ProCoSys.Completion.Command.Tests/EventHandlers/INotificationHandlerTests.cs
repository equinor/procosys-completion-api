using System;
using System.Collections.Generic;
using System.Linq;
using Equinor.ProCoSys.Completion.Domain;
using MediatR;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Equinor.ProCoSys.Completion.Command.Tests.EventHandlers;

[TestClass]
// This test class implement the IDomainMarker to ensure that the Domain Assembly is loaded.
// Reflection is used in test to find all classes implementing INotification
public class INotificationHandlerTests : IDomainMarker
{
    [TestMethod]
    public void AllINotifications_ShouldHaveCorrespondingHandler()
    {
        var allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        var domainAssembly = allAssemblies
            .Single(a => a.FullName != null &&
                         a.FullName.Contains("Equinor.ProCoSys.Completion.Domain") &&
                         !a.FullName.Contains(".Test"));
        var commandAssembly = allAssemblies
            .Single(a => a.FullName != null &&
                         a.FullName.Contains("Equinor.ProCoSys.Completion.Command") &&
                         !a.FullName.Contains(".Test"));

        var notifications =
            domainAssembly.GetTypes()
            .Where(t => typeof(INotification).IsAssignableFrom(t) && t.IsClass && !t.IsAbstract)
            .ToList();

        Assert.AreNotEqual(0, notifications.Count);

        var notificationHandlers = 
            commandAssembly.GetTypes()
            .Where(t =>
                t.GetInterfaces().Any(i =>
                    i.IsGenericType &&
                    i.GetGenericTypeDefinition() == typeof(INotificationHandler<>) &&
                    i.GetGenericArguments()[0].GetInterfaces().Contains(typeof(INotification))
                )
            )
            .ToList();

        var implementedInterfacesInHandlers =
            notificationHandlers.SelectMany(t => t.GetInterfaces()).ToList();

        var genericTypeInHandlers =
            implementedInterfacesInHandlers.SelectMany(i => i.GenericTypeArguments).ToList();

        var missingHandlers = new List<string>();

        foreach (var notification in notifications)
        {
            var notificationHasHandler = genericTypeInHandlers.Any(g => g.FullName == notification.FullName);
            if (!notificationHasHandler)
            {
                missingHandlers.Add(notification.FullName);
            }
        }
    
        Assert.AreEqual(0, missingHandlers.Count, $"{missingHandlers.Count} handlers are missing: {string.Join(",", missingHandlers)}");
    }
}

# 4. contract namespace

Date: 2023-06-27

## Status

Accepted

## Context

We've decided to use MassTransit for publishing / consuming messages on message bus.
MassTransit have this limitation (ref https://masstransit.io/documentation/concepts/messages):

"MassTransit uses the full type name, including the namespace, for message contracts. When creating the same message type in two separate projects, the namespaces must match or the message will not be consumed."


## Decision

We've decided to put all our message contract in namespace named "Equinor.ProCoSys.MessageContracts".
This even though contracts are placed in another folder than "Equinor.ProCoSys.MessageContracts" inside the
Visual Studio project.

## Consequences

MassTransit will create topics based on name of contract, and MassTransit can be used both for publishing / consuming messages on message bus.
The test method Equinor.ProCoSys.Completion.Command.Tests.MessageContracts.AssertNamespaceNotChanged check this "rule".
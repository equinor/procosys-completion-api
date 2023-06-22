# 3. Use MassTransit

Date: 2023-06-22

## Status

Accepted

## Context

Is there a known and recognized framework for publishing / consuming messages on message bus? We like to use Outbox pattern

## Decision

Microsoft suggests MassTransit or similar. The team as evaluated MassTransit through a POC. Seem to be easy to setup, 
including Outbox pattern, MassTransit also create Azure Bus Topics when using Publish.
Link to MS article suggesting MT: https://learn.microsoft.com/en-us/dotnet/architecture/microservices/architect-microservice-container-applications/asynchronous-message-based-communication

## Consequences

We don't need to implement custom code for this

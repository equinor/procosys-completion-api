using System;
using MediatR;

namespace Equinor.ProCoSys.Completion.Command.PersonCommands.CreatePerson;

public class CreatePersonCommand : IRequest<Unit>
{
    public CreatePersonCommand(Guid oid) => Oid = oid;

    public Guid Oid { get; }
}

using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using MediatR;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PersonCommands.CreatePerson;

public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, Result<Unit>>
{
    private readonly IPersonCache _personCache;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptionsMonitor<ApplicationOptions> _options;

    public CreatePersonCommandHandler(
        IPersonCache personCache,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IOptionsMonitor<ApplicationOptions> options)
    {
        _personCache = personCache;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _options = options;
    }

    public async Task<Result<Unit>> Handle(CreatePersonCommand request, CancellationToken cancellationToken)
    {
        var person = await _personRepository.TryGetByGuidAsync(request.Oid);

        if (person == null)
        {
            var pcsPerson = await _personCache.GetAsync(request.Oid);
            if (pcsPerson == null)
            {
                throw new Exception($"Details for user with oid {request.Oid:D} not found in ProCoSys");
            }

            if (!pcsPerson.ServicePrincipal)
            {
                person = new Person(request.Oid, pcsPerson.FirstName, pcsPerson.LastName, pcsPerson.UserName, pcsPerson.Email);
            }
            else
            {
                person = new Person(request.Oid, pcsPerson.FirstName, pcsPerson.LastName, pcsPerson.UserName, _options.CurrentValue.ServicePrincipalMail);
            }
            _personRepository.Add(person);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
            
        return new SuccessResult<Unit>(Unit.Value);
    }
}

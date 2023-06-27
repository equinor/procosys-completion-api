using System;
using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Auth.Caches;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.AggregateModels.PersonAggregate;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ServiceResult;

namespace Equinor.ProCoSys.Completion.Command.PersonCommands.CreatePerson;

public class CreatePersonCommandHandler : IRequestHandler<CreatePersonCommand, Result<Unit>>
{
    private readonly IPersonCache _personCache;
    private readonly IPersonRepository _personRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOptionsMonitor<ApplicationOptions> _options;
    private readonly ILogger<CreatePersonCommandHandler> _logger;

    public CreatePersonCommandHandler(
        IPersonCache personCache,
        IPersonRepository personRepository,
        IUnitOfWork unitOfWork,
        IOptionsMonitor<ApplicationOptions> options,
        ILogger<CreatePersonCommandHandler> logger)
    {
        _personCache = personCache;
        _personRepository = personRepository;
        _unitOfWork = unitOfWork;
        _options = options;
        _logger = logger;
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

            string type;
            if (!pcsPerson.ServicePrincipal)
            {
                person = new Person(request.Oid, pcsPerson.FirstName, pcsPerson.LastName, pcsPerson.UserName, pcsPerson.Email);
                type = "Person";
            }
            else
            {
                person = new Person(request.Oid, pcsPerson.FirstName, pcsPerson.LastName, pcsPerson.UserName, _options.CurrentValue.ServicePrincipalMail);
                type = "ServicePrincipal";
            }
            _personRepository.Add(person);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("{Type} with oid {Oid} created", type, person.Guid);
        }

        return new SuccessResult<Unit>(Unit.Value);
    }
}

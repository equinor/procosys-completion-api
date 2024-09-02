using System.Threading;
using System.Threading.Tasks;
using Equinor.ProCoSys.Completion.Domain;
using Equinor.ProCoSys.Completion.Domain.Validators;
using FluentValidation;

namespace Equinor.ProCoSys.Completion.Query.ProjectQueries.SearchCheckLists;

public class SearchCheckListsQueryValidator : AbstractValidator<SearchCheckListsQuery>
{
    public SearchCheckListsQueryValidator(IProjectValidator projectValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(query => query)
            .MustAsync(BeAnExistingProjectAsync)
            .WithMessage(query =>
                $"Project with this guid does not exist! Guid={query.ProjectGuid}")
            .WithState(_ => new EntityNotFoundException())
            .MustAsync(NotBeAClosedProjectAsync)
            .WithMessage(query =>
                $"Project is closed! Guid={query.ProjectGuid}")
            .Must(BeAValidRegisterAndTagFunctionCode)
            .WithMessage(query =>
                $"RegisterAndTagFunctionCode must be in form X/Y! Guid={query.RegisterAndTagFunctionCode}");

        async Task<bool> BeAnExistingProjectAsync(SearchCheckListsQuery query, CancellationToken cancellationToken)
            => await projectValidator.ExistsAsync(query.ProjectGuid, cancellationToken);

        async Task<bool> NotBeAClosedProjectAsync(SearchCheckListsQuery query, CancellationToken cancellationToken)
            => !await projectValidator.IsClosedAsync(query.ProjectGuid, cancellationToken);

        bool BeAValidRegisterAndTagFunctionCode(SearchCheckListsQuery query)
        {
            if (query.RegisterAndTagFunctionCode is null)
            {
                return true;
            }

            var codes = query.RegisterAndTagFunctionCode.Split("/");
            return codes.Length == 2;
        }
    }
}

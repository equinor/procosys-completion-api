using FluentValidation;

namespace Equinor.ProCoSys.Completion.WebApi.Controllers.Comments;

public class CreateCommentDtoValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentDtoValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(dto => dto).NotNull();

        RuleFor(dto => dto.Text)
            .NotNull()
            .MinimumLength(1)
            .MaximumLength(Domain.AggregateModels.CommentAggregate.Comment.TextLengthMax);
    }
}

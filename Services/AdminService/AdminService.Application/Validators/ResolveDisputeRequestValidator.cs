using FluentValidation;
using AdminService.Application.DTOs.Requests;

namespace AdminService.Application.Validators;

public class ResolveDisputeRequestValidator : AbstractValidator<ResolveDisputeRequest>
{
    public ResolveDisputeRequestValidator()
    {
        RuleFor(x => x.Resolution)
            .NotEmpty().WithMessage("Resolution is required")
            .Must(res => new[] { "Open", "UnderReview", "ResolvedCustomerFavor", "ResolvedRestaurantFavor", "Closed" }.Contains(res))
            .WithMessage("Invalid resolution status");

        RuleFor(x => x.ResolutionNotes)
            .NotEmpty().WithMessage("Resolution notes are required")
            .MinimumLength(10).WithMessage("Resolution notes must be at least 10 characters")
            .MaximumLength(1000).WithMessage("Resolution notes cannot exceed 1000 characters");

        When(x => x.RefundAmount.HasValue, () =>
        {
            RuleFor(x => x.RefundAmount)
                .GreaterThan(0).WithMessage("Refund amount must be greater than 0");
        });
    }
}

using FluentValidation;
using AdminService.Application.DTOs.Requests;

namespace AdminService.Application.Validators;

public class RejectRestaurantRequestValidator : AbstractValidator<RejectRestaurantRequest>
{
    public RejectRestaurantRequestValidator()
    {
        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .MinimumLength(10).WithMessage("Rejection reason must be at least 10 characters")
            .MaximumLength(500).WithMessage("Rejection reason cannot exceed 500 characters");
    }
}

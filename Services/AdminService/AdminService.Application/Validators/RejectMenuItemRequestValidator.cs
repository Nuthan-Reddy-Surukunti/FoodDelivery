using FluentValidation;
using AdminService.Application.DTOs.Requests;

namespace AdminService.Application.Validators;

public class RejectMenuItemRequestValidator : AbstractValidator<RejectMenuItemRequest>
{
    public RejectMenuItemRequestValidator()
    {
        RuleFor(x => x.RejectionReason)
            .NotEmpty().WithMessage("Rejection reason is required")
            .Length(10, 500).WithMessage("Rejection reason must be between 10 and 500 characters");
    }
}
using AdminService.Application.DTOs.Requests;
using FluentValidation;

namespace AdminService.Application.Validators;

public class UpdateOrderStatusRequestValidator : AbstractValidator<UpdateOrderStatusRequest>
{
    public UpdateOrderStatusRequestValidator()
    {
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Reason is required for status changes")
            .MinimumLength(10).WithMessage("Reason must be at least 10 characters")
            .MaximumLength(1000).WithMessage("Reason cannot exceed 1000 characters");

        RuleFor(x => x.NewStatus)
            .IsInEnum().WithMessage("Invalid order status");

        RuleFor(x => x.RefundAmount)
            .GreaterThan(0).WithMessage("Refund amount must be greater than 0")
            .When(x => x.RefundAmount.HasValue);
    }
}
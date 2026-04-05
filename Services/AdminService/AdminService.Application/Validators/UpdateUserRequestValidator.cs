using FluentValidation;
using AdminService.Application.DTOs.Requests;

namespace AdminService.Application.Validators;

public class UpdateUserRequestValidator : AbstractValidator<UpdateUserRequest>
{
    public UpdateUserRequestValidator()
    {
        When(x => x.Email != null, () =>
        {
            RuleFor(x => x.Email)
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(255).WithMessage("Email cannot exceed 255 characters");
        });

        When(x => x.Phone != null, () =>
        {
            RuleFor(x => x.Phone)
                .Matches(@"^[\d\s\-\+\(\)]+$").WithMessage("Invalid phone format")
                .MinimumLength(10).WithMessage("Phone must be at least 10 digits");
        });

        When(x => x.Role != null, () =>
        {
            RuleFor(x => x.Role)
                .Must(role => new[] { "Customer", "Restaurant", "DeliveryAgent", "Admin" }.Contains(role!))
                .WithMessage("Invalid role. Must be Customer, Restaurant, DeliveryAgent, or Admin");
        });
    }
}

using FluentValidation;
using AdminService.Application.DTOs.Requests;

namespace AdminService.Application.Validators;

public class UpdateMenuItemRequestValidator : AbstractValidator<UpdateMenuItemRequest>
{
    public UpdateMenuItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .Length(1, 255).WithMessage("Menu item name must be between 1 and 255 characters")
            .Must(BeValidName).WithMessage("Menu item name cannot have leading or trailing whitespace")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));

        RuleFor(x => x.Description)
            .Length(1, 1000).WithMessage("Menu item description must be between 1 and 1000 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Menu item price must be greater than zero")
            .LessThanOrEqualTo(10000).WithMessage("Menu item price cannot exceed 10,000")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Currency)
            .Length(3).WithMessage("Currency must be a 3-letter ISO code (e.g., USD, EUR, INR)")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be 3 uppercase letters")
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));

        RuleFor(x => x.CategoryId)
            .MaximumLength(100).WithMessage("Category ID cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.CategoryId));

        // If price is provided, currency must also be provided
        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required when price is provided")
            .When(x => x.Price.HasValue);

        // If currency is provided, price must also be provided
        RuleFor(x => x.Price)
            .NotNull().WithMessage("Price is required when currency is provided")
            .When(x => !string.IsNullOrWhiteSpace(x.Currency));
    }

    private static bool BeValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Trim() == name;
    }
}
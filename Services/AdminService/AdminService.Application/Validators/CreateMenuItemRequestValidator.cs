using FluentValidation;
using AdminService.Application.DTOs.Requests;

namespace AdminService.Application.Validators;

public class CreateMenuItemRequestValidator : AbstractValidator<CreateMenuItemRequest>
{
    public CreateMenuItemRequestValidator()
    {
        RuleFor(x => x.RestaurantId)
            .NotEmpty().WithMessage("Restaurant ID is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Menu item name is required")
            .Length(1, 255).WithMessage("Menu item name must be between 1 and 255 characters")
            .Must(BeValidName).WithMessage("Menu item name cannot have leading or trailing whitespace");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Menu item description is required")
            .Length(1, 1000).WithMessage("Menu item description must be between 1 and 1000 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Menu item price must be greater than zero")
            .LessThanOrEqualTo(10000).WithMessage("Menu item price cannot exceed 10,000");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code (e.g., USD, EUR, INR)")
            .Matches("^[A-Z]{3}$").WithMessage("Currency must be 3 uppercase letters");

        RuleFor(x => x.CategoryId)
            .MaximumLength(100).WithMessage("Category ID cannot exceed 100 characters")
            .When(x => !string.IsNullOrEmpty(x.CategoryId));
    }

    private static bool BeValidName(string name)
    {
        return !string.IsNullOrWhiteSpace(name) && name.Trim() == name;
    }
}
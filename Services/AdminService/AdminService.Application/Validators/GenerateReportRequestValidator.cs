using FluentValidation;
using AdminService.Application.DTOs.Requests;

namespace AdminService.Application.Validators;

public class GenerateReportRequestValidator : AbstractValidator<GenerateReportRequest>
{
    public GenerateReportRequestValidator()
    {
        RuleFor(x => x.ReportType)
            .NotEmpty().WithMessage("Report type is required")
            .Must(type => new[] { "Sales", "UserAnalytics", "RestaurantPerformance", "OrderAnalytics", "Revenue", "Custom" }.Contains(type))
            .WithMessage("Invalid report type");

        RuleFor(x => x.StartDate)
            .NotEmpty().WithMessage("Start date is required")
            .LessThan(x => x.EndDate).WithMessage("Start date must be before end date");

        RuleFor(x => x.EndDate)
            .NotEmpty().WithMessage("End date is required")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("End date cannot be in the future");

        When(x => x.ReportType == "RestaurantPerformance", () =>
        {
            RuleFor(x => x.RestaurantId)
                .NotEmpty().WithMessage("Restaurant ID is required for restaurant performance reports");
        });
    }
}

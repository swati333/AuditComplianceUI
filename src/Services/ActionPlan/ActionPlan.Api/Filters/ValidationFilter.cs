using Ehs.SharedKernel.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ActionPlan.Api.Filters;

/// <summary>
/// Centralized FluentValidation wiring (CLAUDE.md §4/§9) — see Finding.Api's
/// identical filter for the full rationale.
/// </summary>
public sealed class ValidationFilter : IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        foreach (var argument in context.ActionArguments.Values)
        {
            if (argument is null)
            {
                continue;
            }

            var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
            if (context.HttpContext.RequestServices.GetService(validatorType) is not IValidator validator)
            {
                continue;
            }

            var validationContext = new ValidationContext<object>(argument);
            var result = await validator.ValidateAsync(validationContext);
            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
                throw new BusinessValidationException(errors);
            }
        }

        await next();
    }
}

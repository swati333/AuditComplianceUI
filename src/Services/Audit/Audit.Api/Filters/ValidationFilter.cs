using Ehs.SharedKernel.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Audit.Api.Filters;

/// <summary>
/// Centralized FluentValidation wiring (CLAUDE.md §4/§9): for every bound
/// action argument that has a registered <c>IValidator&lt;T&gt;</c>, validate
/// it before the action runs. A failure throws <see cref="BusinessValidationException"/>,
/// which the global exception handler (Ehs.Observability) turns into a 400
/// ProblemDetails with a per-field <c>errors</c> map — so individual
/// controller actions and Application-layer methods never call a validator
/// themselves.
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

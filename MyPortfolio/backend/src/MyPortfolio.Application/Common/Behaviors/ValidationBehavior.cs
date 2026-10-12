using FluentValidation;
using MediatR;
using MyPortfolio.Application.Common.Models;
using ValidationException = MyPortfolio.Application.Common.Exceptions.ValidationException;

namespace MyPortfolio.Application.Common.Behaviors;

public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errorMessages = failures.Select(f => f.ErrorMessage).ToList();

            // If TResponse is Result, return failed Result
            if (typeof(TResponse) == typeof(Result))
            {
                return (TResponse)(object)Result.Failure(errorMessages);
            }

            // If TResponse is Result<T>, return failed Result<T>
            if (typeof(TResponse).IsGenericType && typeof(TResponse).GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = typeof(TResponse).GetMethod(
                    nameof(Result<object>.Failure),
                    new[] { typeof(List<string>), typeof(string) });

                if (failureMethod != null)
                {
                    return (TResponse)failureMethod.Invoke(null, new object[] { errorMessages, "Validation failed." })!;
                }
            }

            // Fallback: throw ValidationException for exception pipeline
            throw new ValidationException(failures);
        }

        return await next();
    }
}

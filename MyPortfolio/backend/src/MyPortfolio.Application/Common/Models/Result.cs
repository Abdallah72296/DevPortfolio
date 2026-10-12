namespace MyPortfolio.Application.Common.Models;

public class Result
{
    public bool Succeeded { get; protected set; }
    public string? Message { get; protected set; }
    public List<string> Errors { get; protected set; } = [];

    protected Result()
    {
    }

    public static Result Success(string? message = null)
    {
        return new Result
        {
            Succeeded = true,
            Message = message ?? "Operation completed successfully."
        };
    }

    public static Result Failure(string error, List<string>? errors = null)
    {
        var result = new Result
        {
            Succeeded = false,
            Message = error
        };

        if (errors != null && errors.Count > 0)
        {
            result.Errors.AddRange(errors);
        }
        else
        {
            result.Errors.Add(error);
        }

        return result;
    }

    public static Result Failure(List<string> errors, string message = "Validation or business failure occurred.")
    {
        return new Result
        {
            Succeeded = false,
            Message = message,
            Errors = errors
        };
    }
}

public class Result<T> : Result
{
    public T? Data { get; private set; }

    protected Result()
    {
    }

    public static Result<T> Success(T data, string? message = null)
    {
        return new Result<T>
        {
            Succeeded = true,
            Data = data,
            Message = message ?? "Operation completed successfully."
        };
    }

    public static new Result<T> Failure(string error, List<string>? errors = null)
    {
        var result = new Result<T>
        {
            Succeeded = false,
            Message = error
        };

        if (errors != null && errors.Count > 0)
        {
            result.Errors.AddRange(errors);
        }
        else
        {
            result.Errors.Add(error);
        }

        return result;
    }

    public static new Result<T> Failure(List<string> errors, string message = "Validation or business failure occurred.")
    {
        return new Result<T>
        {
            Succeeded = false,
            Message = message,
            Errors = errors
        };
    }
}

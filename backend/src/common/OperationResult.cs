public class OperationResult
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public string ErrorCode { get; init; } = string.Empty;

    public static OperationResult Success(string message = "")
    {
        return new OperationResult
        {
            IsSuccess = true,
            Message = message
        };
    }

    public static OperationResult Failure(string errorCode, string message)
    {
        return new OperationResult
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            Message = message
        };
    }
}

public class OperationResult<T>
{
    public bool IsSuccess { get; init; }
    public string Message { get; init; } = string.Empty;
    public string ErrorCode { get; init; } = string.Empty;
    public T? Data { get; init; }

    public static OperationResult<T> Success(T data, string message = "")
    {
        return new OperationResult<T>
        {
            IsSuccess = true,
            Data = data,
            Message = message
        };
    }

    public static OperationResult<T> Failure(string errorCode, string message)
    {
        return new OperationResult<T>
        {
            IsSuccess = false,
            ErrorCode = errorCode,
            Message = message
        };
    }
}

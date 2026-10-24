namespace CCS.Entities.Api.Common;

public class Result
{
    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("Un resultado exitoso no puede tener error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("Un resultado fallido debe tener un error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, Error.None);

    public static Result<T> Failure<T>(Error error) => new(default, false, error);
}

/// <summary>Resultado que además entrega un valor cuando la operación tiene éxito.</summary>
public class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, Error error) : base(isSuccess, error) => _value = value;

    /// <summary>Valor obtenido. Solo se puede leer si <see cref="Result.IsSuccess"/> es true.</summary>
    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("No se puede leer el valor de un resultado fallido.");

    /// <summary>Permite devolver un valor directamente: <c>return dto;</c> equivale a <c>Result.Success(dto)</c>.</summary>
    public static implicit operator Result<T>(T value) => Success(value);
}

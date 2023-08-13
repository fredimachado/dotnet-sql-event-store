namespace WareHouseApi;

public readonly struct Result<TValue, TError>
{
    private readonly TValue _value;
    private readonly TError _error;

    public bool IsError { get; }

    public Result(TValue value)
    {
        IsError = false;
        _value = value;
        _error = default;
    }

    public Result(TError error)
    {
        IsError = true;
        _value = default;
        _error = error;
    }

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public TResult Match<TResult>(Func<TValue, TResult> value, Func<TError, TResult> error)
        => _value is not null ? value(_value) : error(_error);
}

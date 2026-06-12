namespace RetailCore.Application.Common.Exceptions;

/// <summary>Base class for expected, mapped-to-HTTP application errors.</summary>
public abstract class AppException : Exception
{
    protected AppException(string message) : base(message) { }
}

/// <summary>Requested resource does not exist (HTTP 404).</summary>
public sealed class NotFoundException : AppException
{
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(string entity, object key) : base($"{entity} '{key}' was not found.") { }
}

/// <summary>Request conflicts with current state, e.g. duplicate unique value (HTTP 409).</summary>
public sealed class ConflictException : AppException
{
    public ConflictException(string message) : base(message) { }
}

/// <summary>A domain/business rule was violated (HTTP 422).</summary>
public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string message) : base(message) { }
}

/// <summary>Authentication failed or credentials are invalid (HTTP 401).</summary>
public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(message) { }
}

/// <summary>Not enough stock to fulfil a sale (HTTP 409). Central to overselling prevention.</summary>
public sealed class InsufficientStockException : AppException
{
    public long ProductId { get; }
    public int Requested { get; }
    public int Available { get; }

    public InsufficientStockException(long productId, int requested, int available)
        : base($"Not enough stock for product {productId}: requested {requested}, available {available}.")
    {
        ProductId = productId;
        Requested = requested;
        Available = available;
    }
}

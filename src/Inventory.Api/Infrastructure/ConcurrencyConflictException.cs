namespace Inventory.Api.Infrastructure;

/// <summary>Someone else changed the row between our read and our write. The API answers 409 and the client may retry.</summary>
public sealed class ConcurrencyConflictException(string message) : Exception(message);

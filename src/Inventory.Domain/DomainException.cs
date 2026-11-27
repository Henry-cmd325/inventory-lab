namespace Inventory.Domain;

/// <summary>A business rule was broken. The API answers 400 unless a subtype says otherwise.</summary>
public class DomainException(string message) : Exception(message);

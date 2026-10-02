namespace Company.Webshop.Shared.Exceptions;

public sealed class BusinessRuleException(string message) : Exception(message);

namespace HorizonLunaris.Api.Services;

// Throw these from services. ApiExceptionHandler turns them into HTTP responses:
//   NotFoundException -> 404, ForbiddenException -> 403, BusinessRuleException -> 409,
//   UnauthorizedAccessException -> 401 (no/unknown acting user), NotImplementedException -> 501.

public class NotFoundException(string message) : Exception(message);

public class ForbiddenException(string message) : Exception(message);

/// <summary>A valid request that breaks a business rule (e.g. circular dependency, starting a blocked task).</summary>
public class BusinessRuleException(string message) : Exception(message);

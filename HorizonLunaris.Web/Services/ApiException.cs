namespace HorizonLunaris.Web.Services;

/// <summary>Thrown when the API returns an error status. Message is the API's explanation.</summary>
public class ApiException(int statusCode, string message) : Exception(message)
{
    public int StatusCode { get; } = statusCode;
}

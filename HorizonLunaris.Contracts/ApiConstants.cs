using System.Text.Json;
using System.Text.Json.Serialization;

namespace HorizonLunaris.Contracts;

public static class ApiHeaders
{
    /// <summary>Identifies the acting user. The prototype has no login; the UI sends this header.</summary>
    public const string UserId = "X-User-Id";
}

public static class ContractJson
{
    /// <summary>JSON settings shared by the API and the web client (camelCase, enums as text).</summary>
    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };
}

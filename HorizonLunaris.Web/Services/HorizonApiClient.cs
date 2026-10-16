using System.Net.Http.Json;
using System.Text.Json;
using HorizonLunaris.Contracts;

namespace HorizonLunaris.Web.Services;

public class HorizonApiClient(HttpClient http) : IHorizonApiClient
{
    private static readonly JsonSerializerOptions Json = ContractJson.Options;

    // ----- Users -----
    public Task<IReadOnlyList<UserDto>> GetUsersAsync(CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<UserDto>>(HttpMethod.Get, "api/users", null, ct);

    // ----- Feature 1 -----
    public Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<ProjectDto>>(HttpMethod.Get, "api/projects", null, ct);

    public Task<ProjectDto> GetProjectAsync(int projectId, CancellationToken ct = default) =>
        SendAsync<ProjectDto>(HttpMethod.Get, $"api/projects/{projectId}", null, ct);

    public Task<ProjectDto> CreateProjectAsync(CreateProjectRequest request, CancellationToken ct = default) =>
        SendAsync<ProjectDto>(HttpMethod.Post, "api/projects", request, ct);

    public Task<IReadOnlyList<TaskDto>> GetTasksAsync(int projectId, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<TaskDto>>(HttpMethod.Get, $"api/projects/{projectId}/tasks", null, ct);

    public Task<TaskDto> CreateTaskAsync(int projectId, CreateTaskRequest request, CancellationToken ct = default) =>
        SendAsync<TaskDto>(HttpMethod.Post, $"api/projects/{projectId}/tasks", request, ct);

    public Task<TaskDto> GetTaskAsync(int taskId, CancellationToken ct = default) =>
        SendAsync<TaskDto>(HttpMethod.Get, $"api/tasks/{taskId}", null, ct);

    public Task<TaskDto> AssignOwnerAsync(int taskId, AssignOwnerRequest request, CancellationToken ct = default) =>
        SendAsync<TaskDto>(HttpMethod.Put, $"api/tasks/{taskId}/owner", request, ct);

    // ----- Feature 2 -----
    public Task<StatusUpdateResult> UpdateStatusAsync(int taskId, UpdateStatusRequest request, CancellationToken ct = default) =>
        SendAsync<StatusUpdateResult>(HttpMethod.Put, $"api/tasks/{taskId}/status", request, ct);

    public Task<IReadOnlyList<CommentDto>> GetCommentsAsync(int taskId, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<CommentDto>>(HttpMethod.Get, $"api/tasks/{taskId}/comments", null, ct);

    public Task<CommentDto> AddCommentAsync(int taskId, AddCommentRequest request, CancellationToken ct = default) =>
        SendAsync<CommentDto>(HttpMethod.Post, $"api/tasks/{taskId}/comments", request, ct);

    // ----- Feature 3 -----
    public Task<IReadOnlyList<TaskDto>> GetPrerequisitesAsync(int taskId, CancellationToken ct = default) =>
        SendAsync<IReadOnlyList<TaskDto>>(HttpMethod.Get, $"api/tasks/{taskId}/dependencies", null, ct);

    public Task<TaskDto> AddDependencyAsync(int taskId, AddDependencyRequest request, CancellationToken ct = default) =>
        SendAsync<TaskDto>(HttpMethod.Post, $"api/tasks/{taskId}/dependencies", request, ct);

    public async Task RemoveDependencyAsync(int taskId, int prerequisiteTaskId, CancellationToken ct = default)
    {
        using var response = await http.DeleteAsync($"api/tasks/{taskId}/dependencies/{prerequisiteTaskId}", ct);
        await EnsureSuccessAsync(response, ct);
    }

    // ----- helpers -----
    private async Task<T> SendAsync<T>(HttpMethod method, string url, object? body, CancellationToken ct)
    {
        using var request = new HttpRequestMessage(method, url);
        if (body is not null)
        {
            request.Content = JsonContent.Create(body, body.GetType(), options: Json);
        }

        using var response = await http.SendAsync(request, ct);
        await EnsureSuccessAsync(response, ct);

        var result = await response.Content.ReadFromJsonAsync<T>(Json, ct);
        return result ?? throw new ApiException((int)response.StatusCode, "The API returned an empty response.");
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode) return;

        var message = $"{(int)response.StatusCode} {response.ReasonPhrase}";
        try
        {
            var text = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(text);
            if (doc.RootElement.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
                message = detail.GetString()!;
            else if (doc.RootElement.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                message = title.GetString()!;
        }
        catch (JsonException)
        {
            // Not a problem-details body; keep the status text.
        }

        throw new ApiException((int)response.StatusCode, message);
    }
}

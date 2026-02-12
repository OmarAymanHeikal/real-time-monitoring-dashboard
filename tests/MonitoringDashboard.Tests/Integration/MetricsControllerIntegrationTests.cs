using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using MonitoringDashboard.WebAPI;

namespace MonitoringDashboard.Tests.Integration;

public class MetricsControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public MetricsControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    private async Task<string> GetAuthTokenAsync()
    {
        var loginRequest = new { userName = "admin", password = "Admin@123" };
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);
        var content = await response.Content.ReadFromJsonAsync<LoginResponse>();
        return content?.AccessToken ?? string.Empty;
    }

    [Fact]
    public async Task GetMetrics_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/metrics/server/1?page=1&pageSize=10");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMetrics_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/metrics/server/1");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMetrics_InvalidServerId_ReturnsOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/metrics/server/99999");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode); // Empty list is still OK
    }

    private class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}

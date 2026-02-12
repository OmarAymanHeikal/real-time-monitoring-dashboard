using System.Net;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using MonitoringDashboard.WebAPI;

namespace MonitoringDashboard.Tests.Integration;

public class ServersControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly WebApplicationFactory<Program> _factory;

    public ServersControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
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
    public async Task GetServers_WithoutAuth_ReturnsUnauthorized()
    {
        // Act
        var response = await _client.GetAsync("/api/v1/servers");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetServers_WithAuth_ReturnsOk()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.GetAsync("/api/v1/servers");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateServer_ValidData_ReturnsCreated()
    {
        // Arrange
        var token = await GetAuthTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        var newServer = new
        {
            name = "Test Server",
            ipAddress = "192.168.1.100",
            description = "Integration test server"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/servers", newServer);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    private class LoginResponse
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}

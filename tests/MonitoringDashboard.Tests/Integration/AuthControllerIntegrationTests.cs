using System.Net;
using System.Net.Http.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using MonitoringDashboard.WebAPI;

namespace MonitoringDashboard.Tests.Integration;

public class AuthControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public AuthControllerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsOkWithToken()
    {
        // Arrange
        var loginRequest = new
        {
            userName = "admin",
            password = "Admin@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var content = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(content);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new
        {
            userName = "admin",
            password = "WrongPassword"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Login_EmptyUserName_ReturnsBadRequest()
    {
        // Arrange
        var loginRequest = new
        {
            userName = "",
            password = "Admin@123"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/auth/login", loginRequest);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}

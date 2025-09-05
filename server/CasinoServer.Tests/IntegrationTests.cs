using Microsoft.AspNetCore.Mvc.Testing;
using System.Net;
using System.Text.Json;
using FluentAssertions;

namespace CasinoServer.Tests;

public class CasinoServerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public CasinoServerIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetStatus_ShouldReturnInitialGameState()
    {
        // Act
        var response = await _client.GetAsync("/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("videoUploaded");
        content.Should().Contain("false"); // Initial state should be false
    }

    [Theory]
    [InlineData("1")]
    [InlineData("2")]
    [InlineData("3")]
    public async Task VideoUpload_WithValidDevice_ShouldReturnOk(string deviceId)
    {
        // Act
        var response = await _client.PostAsync($"/video/upload?device={deviceId}", null);

        // Assert
        if (deviceId == "1")
        {
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
        else
        {
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }
    }

    [Fact]
    public async Task VideoUpload_WithDevice1_ShouldUpdateGameState()
    {
        // Act
        await _client.PostAsync("/admin/reset", null); // Reset state first
        var uploadResponse = await _client.PostAsync("/video/upload?device=1", null);
        var statusResponse = await _client.GetAsync("/status");

        // Assert
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusContent = await statusResponse.Content.ReadAsStringAsync();
        statusContent.Should().Contain("\"videoUploaded\":true");
    }

    [Fact]
    public async Task VideoLoop_WithoutUpload_ShouldReturnBadRequest()
    {
        // Arrange
        await _client.PostAsync("/admin/reset", null); // Reset state first

        // Act
        var response = await _client.PostAsync("/video/loop?device=3", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Video not uploaded yet");
    }

    [Fact]
    public async Task VideoLoop_AfterUpload_ShouldReturnOk()
    {
        // Arrange
        await _client.PostAsync("/admin/reset", null); // Reset state first
        await _client.PostAsync("/video/upload?device=1", null);

        // Act
        var response = await _client.PostAsync("/video/loop?device=3", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task LLMPassword_WithDevice1_ShouldUpdatePasswordState()
    {
        // Arrange
        await _client.PostAsync("/admin/reset", null); // Reset state first

        // Act
        var passwordResponse = await _client.PostAsync("/llm/password?device=1", null);
        var statusResponse = await _client.GetAsync("/status");

        // Assert
        passwordResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusContent = await statusResponse.Content.ReadAsStringAsync();
        statusContent.Should().Contain("\"passwordUpdated\":true");
    }

    [Fact]
    public async Task LLMIsUpdated_WithoutPasswordUpdate_ShouldReturnBadRequest()
    {
        // Arrange
        await _client.PostAsync("/admin/reset", null); // Reset state first

        // Act
        var response = await _client.GetAsync("/llm/isupdated?device=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Password not updated yet");
    }

    [Fact]
    public async Task LLMIsUpdated_AfterPasswordUpdate_ShouldReturnOk()
    {
        // Arrange
        await _client.PostAsync("/admin/reset", null); // Reset state first
        await _client.PostAsync("/llm/password?device=1", null);

        // Act
        var response = await _client.GetAsync("/llm/isupdated?device=2");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData("/disco/lights", "1", "2")]
    [InlineData("/disco/music", "1", "3")]
    public async Task DiscoEndpoints_WithWrongDevice_ShouldReturnBadRequest(string endpoint, string wrongDevice, string correctDevice)
    {
        // Act
        var wrongResponse = await _client.PostAsync($"{endpoint}?device={wrongDevice}", null);
        var correctResponse = await _client.PostAsync($"{endpoint}?device={correctDevice}", null);

        // Assert
        wrongResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        correctResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DiscoLights_WithDevice2_ShouldReturnTimeWindow()
    {
        // Arrange
        await _client.PostAsync("/admin/reset", null); // Reset state first

        // Act
        var response = await _client.PostAsync("/disco/lights?device=2", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("5"); // Default disco window is 5 seconds
    }

    [Fact]
    public async Task DiscoMusic_WithDevice3_ShouldReturnTimeWindow()
    {
        // Arrange
        await _client.PostAsync("/admin/reset", null); // Reset state first

        // Act
        var response = await _client.PostAsync("/disco/music?device=3", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("5"); // Default disco window is 5 seconds
    }

    [Fact]
    public async Task DiscoStatus_InitialState_ShouldReturnNotCompleted()
    {
        // Arrange
        await _client.PostAsync("/admin/reset", null); // Reset state first

        // Act
        var response = await _client.GetAsync("/disco/status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"discoCompleted\":false");
    }

    [Fact]
    public async Task DiscoCompletion_WhenBothActivatedWithinWindow_ShouldComplete()
    {
        // Arrange
        await _client.PostAsync("/admin/reset", null); // Reset state first

        // Act
        await _client.PostAsync("/disco/lights?device=2", null);
        await Task.Delay(1000); // Wait 1 second (within 5s window)
        await _client.PostAsync("/disco/music?device=3", null);

        var statusResponse = await _client.GetAsync("/disco/status");

        // Assert
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await statusResponse.Content.ReadAsStringAsync();
        content.Should().Contain("\"discoCompleted\":true");
    }

    [Fact]
    public async Task AdminReset_ShouldResetAllGameState()
    {
        // Arrange - Set some state first
        await _client.PostAsync("/video/upload?device=1", null);
        await _client.PostAsync("/llm/password?device=1", null);
        await _client.PostAsync("/disco/lights?device=2", null);

        // Act
        var resetResponse = await _client.PostAsync("/admin/reset", null);
        var statusResponse = await _client.GetAsync("/status");

        // Assert
        resetResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var statusContent = await statusResponse.Content.ReadAsStringAsync();
        statusContent.Should().Contain("\"videoUploaded\":false");
        statusContent.Should().Contain("\"passwordUpdated\":false");
        statusContent.Should().Contain("\"discoCompleted\":false");
    }

    [Fact]
    public async Task AdminSave_ShouldReturnOk()
    {
        // Act
        var response = await _client.PostAsync("/admin/save", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Game state manually saved");
    }

    [Fact]
    public async Task AdminDeleteSavedState_ShouldReturnOk()
    {
        // Act
        var response = await _client.DeleteAsync("/admin/savedstate");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("Saved state file deleted");
    }

    [Theory]
    [InlineData("/video/upload")]
    [InlineData("/video/loop")]
    [InlineData("/llm/password")]
    [InlineData("/disco/lights")]
    [InlineData("/disco/music")]
    public async Task Endpoints_WithoutDeviceParameter_ShouldHandleGracefully(string endpoint)
    {
        // Act
        var response = await _client.PostAsync(endpoint, null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Theory]
    [InlineData("/llm/isupdated")]
    public async Task GetEndpoints_WithoutDeviceParameter_ShouldHandleGracefully(string endpoint)
    {
        // Act
        var response = await _client.GetAsync(endpoint);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GameState_OverwriteDiscoState_ShouldNotAffectCompletion()
    {
        await _client.PostAsync("/admin/reset", null); // Reset state first

        // Act
        await _client.PostAsync("/disco/lights?device=2", null);
        await Task.Delay(1000); // Wait 1 second (within 5s window)
        await _client.PostAsync("/disco/music?device=3", null);
        await Task.Delay(6000);
        await _client.PostAsync("/disco/music?device=3", null);

        var statusResponse = await _client.GetAsync("/disco/status");

        // Assert
        statusResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await statusResponse.Content.ReadAsStringAsync();
        content.Should().Contain("\"discoCompleted\":true");
    }
}

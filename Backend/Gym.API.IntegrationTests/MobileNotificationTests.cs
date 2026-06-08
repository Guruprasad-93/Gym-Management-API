using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.DTOs.Mobile;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class MobileNotificationTests : IClassFixture<MobileNotificationFixture>
{
    private readonly GymWebApplicationFactory _factory;
    private readonly MobileNotificationFixture _fixture;

    public MobileNotificationTests(GymWebApplicationFactory factory, MobileNotificationFixture fixture)
    {
        _factory = factory;
        _fixture = fixture;
    }

    [Fact]
    public async Task RegisterDevice_ReturnsOk()
    {
        var response = await _fixture.MemberClient.PostAsJsonAsync("/api/mobile/device/register", new RegisterDeviceDto
        {
            DeviceType = "Android",
            DeviceToken = $"test-token-{Guid.NewGuid():N}",
            AppVersion = "1.0.0"
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetMobileDashboard_ReturnsOk()
    {
        var response = await _fixture.MemberClient.GetAsync("/api/mobile/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetSync_ReturnsOk()
    {
        var response = await _fixture.MemberClient.GetAsync("/api/mobile/sync");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdatePreferences_ReturnsOk()
    {
        var response = await _fixture.MemberClient.PutAsJsonAsync("/api/mobile/preferences", new UpdateNotificationPreferencesDto
        {
            PushEnabled = true,
            MembershipReminders = true,
            WorkoutReminders = true,
            DietReminders = true,
            AttendanceReminders = true,
            PromotionalNotifications = false
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetNotifications_ReturnsOk()
    {
        var response = await _fixture.MemberClient.GetAsync("/api/mobile/notifications?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task AdminAnalytics_ReturnsOk()
    {
        var response = await _fixture.AdminClient.GetAsync("/api/mobile/admin/analytics");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_CannotAccessMobileDashboard()
    {
        var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/api/mobile/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MarkNotificationsRead_ReturnsOk()
    {
        var response = await _fixture.MemberClient.PutAsJsonAsync("/api/mobile/notifications/read", new MarkNotificationsReadDto());
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}

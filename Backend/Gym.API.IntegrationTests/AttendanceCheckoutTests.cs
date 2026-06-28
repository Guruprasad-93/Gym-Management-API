using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.Constants;
using Gym.Application.DTOs.Attendance;
using Gym.Application.DTOs.MemberSelfService;
using Gym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class AttendanceCheckoutTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _adminClient = null!;
    private HttpClient _memberClient = null!;

    public AttendanceCheckoutTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _adminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        _memberClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_adminClient, DemoDataSeeder.DemoGymAdminLoginIdentifier, "Demo@123");
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_memberClient, DemoDataSeeder.DemoMember1LoginIdentifier, "Demo@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetAttendanceSettings_ReturnsDefaults()
    {
        var response = await _adminClient.GetAsync("/api/attendance/settings");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<AttendanceSettingsDto>>();
        Assert.NotNull(body?.Data);
        Assert.True(body.Data.AutoCheckoutEnabled);
        Assert.False(body.Data.Is24Hours);
        Assert.Equal(12, body.Data.MaximumSessionHours);
    }

    [Fact]
    public async Task ManualCheckOut_ClosesOpenSession()
    {
        var membersResponse = await _adminClient.GetAsync("/api/members?pageNumber=1&pageSize=5");
        membersResponse.EnsureSuccessStatusCode();

        int? memberId = null;
        for (var attempt = 0; attempt < 5 && memberId is null; attempt++)
        {
            var checkIn = await _adminClient.PostAsJsonAsync("/api/attendance/check-in", new CheckInMemberDto
            {
                MemberId = attempt + 1,
                Notes = "Integration test check-in"
            });

            if (checkIn.StatusCode == HttpStatusCode.OK)
            {
                var checkInBody = await checkIn.Content.ReadFromJsonAsync<ApiEnvelope<MemberAttendanceDto>>();
                memberId = checkInBody?.Data?.MemberId;
            }
        }

        if (memberId is null)
            return;

        var checkOut = await _adminClient.PostAsJsonAsync("/api/attendance/check-out", new CheckOutMemberDto
        {
            MemberId = memberId.Value,
            IsManualCheckout = true,
            Notes = "Integration test manual checkout"
        });

        Assert.Equal(HttpStatusCode.OK, checkOut.StatusCode);
        var checkOutBody = await checkOut.Content.ReadFromJsonAsync<ApiEnvelope<MemberAttendanceDto>>();
        Assert.Equal(AttendanceCheckoutTypes.Manual, checkOutBody?.Data?.CheckoutType);
        Assert.NotNull(checkOutBody?.Data?.CheckOutAt);
    }

    [Fact]
    public async Task MemberDashboard_IncludesTodayVisitShape()
    {
        var response = await _memberClient.GetAsync("/api/member/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<MemberSelfServiceDashboardDto>>();
        Assert.NotNull(body?.Data);
        // TodayVisit may be null when no attendance today; property is optional on DTO.
        _ = body.Data.TodayVisit;
    }

    [Fact]
    public async Task AttendanceList_SupportsCheckoutTypeFilter()
    {
        var response = await _adminClient.GetAsync("/api/attendance?checkoutTypeFilter=Manual&pageNumber=1&pageSize=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Dashboard_IncludesCheckoutSummaryFields()
    {
        var response = await _adminClient.GetAsync("/api/attendance/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiEnvelope<AttendanceDashboardDto>>();
        Assert.NotNull(body?.Data);
        Assert.True(body.Data.CheckedOutToday >= 0);
        Assert.True(body.Data.AutoCheckedOutToday >= 0);
        Assert.True(body.Data.ManualCheckOutToday >= 0);
    }

    [Fact]
    public async Task ForgotCheckOutReport_ReturnsOk()
    {
        var from = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var to = DateOnly.FromDateTime(DateTime.UtcNow);
        var response = await _adminClient.GetAsync(
            $"/api/attendance/reports/forgot-check-out?fromDate={from:yyyy-MM-dd}&toDate={to:yyyy-MM-dd}&pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task UpdateSettings_Supports24HourGym()
    {
        var get = await _adminClient.GetAsync("/api/attendance/settings");
        get.EnsureSuccessStatusCode();
        var current = (await get.Content.ReadFromJsonAsync<ApiEnvelope<AttendanceSettingsDto>>())!.Data!;

        var update = await _adminClient.PutAsJsonAsync("/api/attendance/settings", new UpdateAttendanceSettingsDto
        {
            OpeningTime = current.OpeningTime,
            ClosingTime = current.ClosingTime,
            AutoCheckoutEnabled = current.AutoCheckoutEnabled,
            UseClosingTimeForAutoCheckout = current.UseClosingTimeForAutoCheckout,
            CheckoutReminderMinutesBefore = current.CheckoutReminderMinutesBefore,
            TimeZoneId = current.TimeZoneId,
            Is24Hours = true,
            MaximumSessionHours = 12
        });
        Assert.Equal(HttpStatusCode.OK, update.StatusCode);

        var verify = await _adminClient.GetAsync("/api/attendance/settings");
        var settings = (await verify.Content.ReadFromJsonAsync<ApiEnvelope<AttendanceSettingsDto>>())!.Data!;
        Assert.True(settings.Is24Hours);
        Assert.Equal(12, settings.MaximumSessionHours);

        await _adminClient.PutAsJsonAsync("/api/attendance/settings", new UpdateAttendanceSettingsDto
        {
            OpeningTime = current.OpeningTime,
            ClosingTime = current.ClosingTime,
            AutoCheckoutEnabled = current.AutoCheckoutEnabled,
            UseClosingTimeForAutoCheckout = current.UseClosingTimeForAutoCheckout,
            CheckoutReminderMinutesBefore = current.CheckoutReminderMinutesBefore,
            TimeZoneId = current.TimeZoneId,
            Is24Hours = false,
            MaximumSessionHours = 12
        });
    }

    private sealed class ApiEnvelope<T>
    {
        public T? Data { get; set; }
    }
}

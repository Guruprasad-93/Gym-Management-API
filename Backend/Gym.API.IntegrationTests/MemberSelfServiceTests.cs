using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.DTOs.MemberSelfService;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using Gym.Infrastructure.Persistence;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class MemberSelfServiceTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _memberClient = null!;
    private HttpClient _adminClient = null!;

    public MemberSelfServiceTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _memberClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        _adminClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_memberClient, DemoDataSeeder.DemoMember1LoginIdentifier, "Demo@123");
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_adminClient, DemoDataSeeder.DemoGymAdminLoginIdentifier, "Demo@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetDashboard_ReturnsOk()
    {
        var response = await _memberClient.GetAsync("/api/member/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GoalCrud_ReturnsExpectedStatusCodes()
    {
        var create = await _memberClient.PostAsJsonAsync("/api/member/goals", new CreateMemberGoalDto
        {
            GoalType = GoalTypes.WeightLoss,
            TargetValue = 75,
            CurrentValue = 82,
            TargetDate = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(3))
        });
        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var created = await create.Content.ReadFromJsonAsync<ApiEnvelope<MemberGoalDto>>();
        Assert.NotNull(created?.Data);

        var list = await _memberClient.GetAsync("/api/member/goals");
        Assert.Equal(HttpStatusCode.OK, list.StatusCode);

        var progress = await _memberClient.PatchAsJsonAsync($"/api/member/goals/{created!.Data!.GoalId}/progress", new UpdateGoalProgressDto { CurrentValue = 80 });
        Assert.Equal(HttpStatusCode.OK, progress.StatusCode);

        var complete = await _memberClient.PostAsync($"/api/member/goals/{created.Data.GoalId}/complete", null);
        Assert.Equal(HttpStatusCode.OK, complete.StatusCode);
    }

    [Fact]
    public async Task ProgressTracking_ReturnsCreated()
    {
        var response = await _memberClient.PostAsJsonAsync("/api/member/progress", new CreateMemberProgressDto
        {
            ProgressDate = DateOnly.FromDateTime(DateTime.UtcNow),
            Weight = 78.5m,
            Waist = 84m
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task ReferralTracking_ReturnsOk()
    {
        var response = await _memberClient.GetAsync("/api/member/referrals");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<ApiEnvelope<ReferralStatsDto>>();
        Assert.False(string.IsNullOrWhiteSpace(json?.Data?.ReferralCode));
    }

    [Fact]
    public async Task GetQrCode_ReturnsOkWithPayload()
    {
        var response = await _memberClient.GetAsync("/api/member/qr-code");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<ApiEnvelope<MemberQrCodeDto>>();
        Assert.StartsWith("GMS:", json?.Data?.Payload);
        Assert.False(string.IsNullOrWhiteSpace(json?.Data?.QrCodeBase64));
    }

    [Fact]
    public async Task QrCheckIn_AdminCanScanMemberQr()
    {
        var qr = await _memberClient.GetAsync("/api/member/qr-code");
        qr.EnsureSuccessStatusCode();
        var qrData = (await qr.Content.ReadFromJsonAsync<ApiEnvelope<MemberQrCodeDto>>())!.Data!;

        var checkIn = await _adminClient.PostAsJsonAsync("/api/member/attendance/qr-scan", new QrCheckInDto { QrPayload = qrData.Payload });
        Assert.True(checkIn.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest);

        if (checkIn.StatusCode == HttpStatusCode.OK)
        {
            var checkInBody = await checkIn.Content.ReadFromJsonAsync<ApiEnvelope<QrScanResultDto>>();
            Assert.True(checkInBody?.Data?.AttendanceId > 0);
            Assert.False(string.IsNullOrWhiteSpace(checkInBody?.Data?.MemberName));

            var duplicate = await _adminClient.PostAsJsonAsync("/api/member/attendance/qr-scan", new QrCheckInDto { QrPayload = qrData.Payload });
            Assert.Equal(HttpStatusCode.BadRequest, duplicate.StatusCode);
        }

        var invalid = await _adminClient.PostAsJsonAsync("/api/member/attendance/qr-scan", new QrCheckInDto { QrPayload = "not-a-valid-qr" });
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
    }

    [Fact]
    public async Task GymAdmin_CannotAccessMemberDashboardAsMemberEndpoint()
    {
        var response = await _adminClient.GetAsync("/api/member/dashboard");
        Assert.True(response.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Anonymous_CannotAccessMemberSelfService()
    {
        var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/api/member/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class ApiEnvelope<T>
    {
        public T? Data { get; set; }
    }
}

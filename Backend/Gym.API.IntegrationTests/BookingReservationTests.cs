using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.DTOs.Booking;
using Gym.Application.DTOs.Branches;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.MemberSelfService;
using Gym.Application.DTOs.Trainers;
using Gym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class BookingReservationTests : IClassFixture<BookingReservationFixture>
{
    private readonly GymWebApplicationFactory _factory;
    private readonly BookingReservationFixture _fixture;
    private int? _scheduleId;
    private int? _scheduleDayOfWeek;

    public BookingReservationTests(GymWebApplicationFactory factory, BookingReservationFixture fixture)
    {
        _factory = factory;
        _fixture = fixture;
    }

    private async Task<(int BranchId, int TrainerId)> EnsureBranchAndTrainerAsync()
    {
        var branchesResp = await _fixture.AdminClient.GetAsync("/api/branches/list");
        branchesResp.EnsureSuccessStatusCode();
        var branches = await branchesResp.Content.ReadFromJsonAsync<ApiWrapper<List<BranchDto>>>();
        var branchId = branches?.Data?.FirstOrDefault()?.BranchId;
        if (branchId is null)
        {
            var createBranch = await _fixture.AdminClient.PostAsJsonAsync("/api/branches", new CreateBranchDto
            {
                BranchName = "Booking Test Branch",
                BranchCode = "BTB",
                City = "Demo"
            });
            createBranch.EnsureSuccessStatusCode();
            var created = await createBranch.Content.ReadFromJsonAsync<ApiWrapper<BranchDto>>();
            branchId = created!.Data!.BranchId;
        }

        var trainersResp = await _fixture.AdminClient.GetAsync("/api/trainers?pageNumber=1&pageSize=1");
        trainersResp.EnsureSuccessStatusCode();
        var trainers = await trainersResp.Content.ReadFromJsonAsync<ApiWrapper<PagedResultDto<TrainerDto>>>();
        var trainerId = trainers?.Data?.Items.FirstOrDefault()?.Id
            ?? throw new InvalidOperationException("No trainer available for booking tests.");

        return (branchId.Value, trainerId);
    }

    private async Task<int> EnsureScheduleAsync()
    {
        if (_scheduleId.HasValue) return _scheduleId.Value;
        var (branchId, trainerId) = await EnsureBranchAndTrainerAsync();
        _scheduleDayOfWeek = (int)DateTime.UtcNow.AddDays(21).DayOfWeek;
        var response = await _fixture.AdminClient.PostAsJsonAsync("/api/schedules", new CreateClassScheduleDto
        {
            BranchId = branchId,
            ClassName = $"Test Yoga {Guid.NewGuid():N}",
            Description = "Integration test class",
            TrainerId = trainerId,
            DayOfWeek = _scheduleDayOfWeek.Value,
            StartTime = new TimeSpan(10, 0, 0),
            EndTime = new TimeSpan(11, 0, 0),
            Capacity = 5
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiWrapper<ClassScheduleDto>>();
        _scheduleId = body!.Data!.Id;
        return _scheduleId.Value;
    }

    private static DateTime GetBookingDateForDayOfWeek(int dayOfWeek, int minDaysFromNow = 21)
    {
        var date = DateTime.UtcNow.Date.AddDays(minDaysFromNow);
        while ((int)date.DayOfWeek != dayOfWeek)
            date = date.AddDays(1);
        return date;
    }

    private async Task<int> BookSlotAsync(int scheduleId, DateTime bookingDate)
    {
        var response = await _fixture.MemberClient.PostAsJsonAsync("/api/bookings/book", new BookSlotDto
        {
            ClassScheduleId = scheduleId,
            BookingDate = bookingDate
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ApiWrapper<SlotBookingDto>>();
        Assert.True(body?.Data?.Id > 0);
        return body!.Data!.Id;
    }

    [Fact]
    public async Task CreateSchedule_ReturnsOk()
    {
        var id = await EnsureScheduleAsync();
        Assert.True(id > 0);
    }

    [Fact]
    public async Task GetSchedules_ReturnsOk()
    {
        var response = await _fixture.AdminClient.GetAsync("/api/schedules?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAvailableSlots_ReturnsOk()
    {
        var from = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.AddDays(14).ToString("yyyy-MM-dd");
        var response = await _fixture.MemberClient.GetAsync($"/api/bookings/available-slots?fromDate={from}&toDate={to}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task BookSlot_ReturnsOk()
    {
        var scheduleId = await EnsureScheduleAsync();
        var bookingDate = GetBookingDateForDayOfWeek(_scheduleDayOfWeek!.Value);
        var bookingId = await BookSlotAsync(scheduleId, bookingDate);
        Assert.True(bookingId > 0);
    }

    [Fact]
    public async Task DuplicateBooking_IsRejected()
    {
        var scheduleId = await EnsureScheduleAsync();
        var bookingDate = GetBookingDateForDayOfWeek(_scheduleDayOfWeek!.Value, minDaysFromNow: 28);
        await BookSlotAsync(scheduleId, bookingDate);

        var response = await _fixture.MemberClient.PostAsJsonAsync("/api/bookings/book", new BookSlotDto
        {
            ClassScheduleId = scheduleId,
            BookingDate = bookingDate
        });
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task JoinWaitlist_ReturnsOkOrBadRequest()
    {
        var scheduleId = await EnsureScheduleAsync();
        var bookingDate = GetBookingDateForDayOfWeek(_scheduleDayOfWeek!.Value, minDaysFromNow: 35);
        var response = await _fixture.MemberClient.PostAsJsonAsync("/api/bookings/waitlist", new JoinWaitlistDto
        {
            ClassScheduleId = scheduleId,
            BookingDate = bookingDate
        });
        Assert.True(response.StatusCode is HttpStatusCode.OK or HttpStatusCode.BadRequest or HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CancelBooking_ReturnsOk()
    {
        var scheduleId = await EnsureScheduleAsync();
        var bookingDate = GetBookingDateForDayOfWeek(_scheduleDayOfWeek!.Value, minDaysFromNow: 42);
        var bookingId = await BookSlotAsync(scheduleId, bookingDate);

        var response = await _fixture.MemberClient.PostAsJsonAsync("/api/bookings/cancel", new CancelBookingDto { BookingId = bookingId });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetTrainerSchedule_ReturnsOk()
    {
        var from = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var to = DateTime.UtcNow.AddDays(30).ToString("yyyy-MM-dd");
        var response = await _fixture.TrainerClient.GetAsync($"/api/trainer-schedule?fromDate={from}&toDate={to}");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetBookingAnalytics_ReturnsOk()
    {
        var response = await _fixture.AdminClient.GetAsync("/api/booking-analytics?days=30");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Anonymous_CannotAccessBookings()
    {
        var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/api/bookings?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task MemberCannotManageSchedules()
    {
        var (branchId, trainerId) = await EnsureBranchAndTrainerAsync();
        var response = await _fixture.MemberClient.PostAsJsonAsync("/api/schedules", new CreateClassScheduleDto
        {
            BranchId = branchId,
            ClassName = "Unauthorized",
            TrainerId = trainerId,
            DayOfWeek = 1,
            StartTime = TimeSpan.FromHours(9),
            EndTime = TimeSpan.FromHours(10),
            Capacity = 5
        });
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task BookingQrCheckIn_ReturnsOkWithMemberAndBookingDetails()
    {
        await _fixture.AdminClient.PutAsJsonAsync("/api/bookings/settings", new UpdateBookingSettingsDto
        {
            MaxBookingsPerDay = 20,
            AllowWaitlist = true,
            CancellationWindowHours = 2,
            ReminderMinutesBefore = 60
        });

        var qrResponse = await _fixture.MemberClient.GetAsync("/api/member/qr-code");
        qrResponse.EnsureSuccessStatusCode();
        var qrData = (await qrResponse.Content.ReadFromJsonAsync<ApiWrapper<MemberQrCodeDto>>())!.Data!;

        var (branchId, trainerId) = await EnsureBranchAndTrainerAsync();
        var todayDow = (int)DateTime.UtcNow.DayOfWeek;
        var createSchedule = await _fixture.AdminClient.PostAsJsonAsync("/api/schedules", new CreateClassScheduleDto
        {
            BranchId = branchId,
            ClassName = $"QR Check-In {Guid.NewGuid():N}",
            TrainerId = trainerId,
            DayOfWeek = todayDow,
            StartTime = new TimeSpan(6, 0, 0),
            EndTime = new TimeSpan(23, 0, 0),
            Capacity = 10
        });
        createSchedule.EnsureSuccessStatusCode();
        var schedule = (await createSchedule.Content.ReadFromJsonAsync<ApiWrapper<ClassScheduleDto>>())!.Data!;

        var bookResponse = await _fixture.MemberClient.PostAsJsonAsync("/api/bookings/book", new BookSlotDto
        {
            ClassScheduleId = schedule.Id,
            BookingDate = DateTime.UtcNow.Date
        });
        Assert.Equal(HttpStatusCode.OK, bookResponse.StatusCode);

        var checkIn = await _fixture.AdminClient.PostAsJsonAsync("/api/booking-checkin", new BookingCheckInDto
        {
            QrPayload = qrData.Payload
        });
        Assert.Equal(HttpStatusCode.OK, checkIn.StatusCode);
        var result = await checkIn.Content.ReadFromJsonAsync<ApiWrapper<QrScanResultDto>>();
        Assert.True(result?.Data?.BookingId > 0);
        Assert.Equal("CheckedIn", result?.Data?.BookingStatus);
        Assert.False(string.IsNullOrWhiteSpace(result?.Data?.MemberName));
        Assert.False(string.IsNullOrWhiteSpace(result?.Data?.ClassName));
    }

    [Fact]
    public async Task GetMemberBookings_ReturnsOk()
    {
        var response = await _fixture.MemberClient.GetAsync("/api/bookings?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task DeleteSchedule_RemovesScheduleAndBookings()
    {
        var (branchId, trainerId) = await EnsureBranchAndTrainerAsync();
        var dayOfWeek = (int)DateTime.UtcNow.AddDays(42).DayOfWeek;
        var createResponse = await _fixture.AdminClient.PostAsJsonAsync("/api/schedules", new CreateClassScheduleDto
        {
            BranchId = branchId,
            ClassName = $"Delete Test {Guid.NewGuid():N}",
            TrainerId = trainerId,
            DayOfWeek = dayOfWeek,
            StartTime = new TimeSpan(14, 0, 0),
            EndTime = new TimeSpan(15, 0, 0),
            Capacity = 5
        });
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<ApiWrapper<ClassScheduleDto>>();
        var scheduleId = created!.Data!.Id;

        var bookingDate = GetBookingDateForDayOfWeek(dayOfWeek, minDaysFromNow: 200);
        var bookingId = await BookSlotAsync(scheduleId, bookingDate);

        var deleteResponse = await _fixture.AdminClient.DeleteAsync($"/api/schedules/{scheduleId}");
        Assert.Equal(HttpStatusCode.OK, deleteResponse.StatusCode);

        var getSchedule = await _fixture.AdminClient.GetAsync($"/api/schedules/{scheduleId}");
        Assert.Equal(HttpStatusCode.NotFound, getSchedule.StatusCode);

        var bookings = await _fixture.AdminClient.GetAsync("/api/bookings?pageNumber=1&pageSize=500");
        bookings.EnsureSuccessStatusCode();
        var body = await bookings.Content.ReadFromJsonAsync<ApiWrapper<PagedResultDto<SlotBookingDto>>>();
        Assert.DoesNotContain(body!.Data!.Items, b => b.Id == bookingId);
    }

    private sealed class ApiWrapper<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
    }
}

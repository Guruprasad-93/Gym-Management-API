using System.Net;
using System.Net.Http.Json;
using Gym.API.IntegrationTests.Infrastructure;
using Gym.Application.DTOs.Financial;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Gym.API.IntegrationTests;

[Collection(nameof(IntegrationTestCollection))]
public class FinancialManagementTests : IAsyncLifetime
{
    private readonly GymWebApplicationFactory _factory;
    private HttpClient _client = null!;

    public FinancialManagementTests(GymWebApplicationFactory factory) => _factory = factory;

    public async Task InitializeAsync()
    {
        await _factory.EnsureDatabaseAsync();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { HandleCookies = true });
        await AuthenticatedClientHelper.CreateAuthenticatedClientAsync(_client, "admin@fitzone-demo.com", "Demo@123");
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetExpenseCategories_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/expenses/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task CreateExpense_ReturnsCreated()
    {
        var categories = await _client.GetAsync("/api/expenses/categories");
        categories.EnsureSuccessStatusCode();
        var catJson = await categories.Content.ReadFromJsonAsync<ApiEnvelope<List<ExpenseCategoryDto>>>();
        var categoryId = catJson?.Data?.FirstOrDefault()?.Id ?? 1;

        var response = await _client.PostAsJsonAsync("/api/expenses", new CreateExpenseDto
        {
            CategoryId = categoryId,
            Amount = 1500,
            ExpenseDate = DateOnly.FromDateTime(DateTime.UtcNow),
            PaymentMethod = "Cash",
            Description = "Integration test expense"
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task GetExpensesPaged_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/expenses?pageNumber=1&pageSize=10");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GeneratePayroll_ReturnsOk()
    {
        var month = new DateOnly(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var response = await _client.PostAsJsonAsync("/api/payroll/generate", new GeneratePayrollDto
        {
            SalaryMonth = month,
            DefaultTrainerBaseSalary = 15000,
            DefaultStaffBaseSalary = 25000
        });
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetFinancialDashboard_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/financial/dashboard");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetProfitLoss_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/financial/profit-loss");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GymAdmin_CannotAccessExpenses_WithWrongGymId()
    {
        var wrongGymId = Guid.NewGuid();
        var response = await _client.GetAsync($"/api/expenses?gymId={wrongGymId}&pageNumber=1&pageSize=10");
        Assert.True(
            response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden
                or HttpStatusCode.BadRequest or HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Anonymous_CannotAccessFinancial()
    {
        var anon = _factory.CreateClient();
        var response = await anon.GetAsync("/api/financial/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private sealed class ApiEnvelope<T>
    {
        public T? Data { get; set; }
    }
}

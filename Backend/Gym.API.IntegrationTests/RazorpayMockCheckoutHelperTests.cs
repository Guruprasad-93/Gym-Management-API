using Gym.Application.Payments;
using Xunit;

namespace Gym.API.IntegrationTests;

public class RazorpayMockCheckoutHelperTests
{
    [Fact]
    public void CreateSuccess_ProducesSignatureAcceptedByMockGateway()
    {
        const string orderId = "order_mock_test123";
        var (paymentId, signature) = RazorpayMockCheckoutHelper.CreateSuccess(orderId, RazorpayMockCheckoutHelper.DevKeySecret);

        var gateway = new Gym.Infrastructure.Services.MockRazorpayGateway();
        Assert.True(gateway.VerifyPaymentSignature(orderId, paymentId, signature));
    }
}

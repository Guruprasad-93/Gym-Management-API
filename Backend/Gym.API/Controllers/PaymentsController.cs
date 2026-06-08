using Gym.Application.Authorization;
using Gym.Application.Constants;
using Gym.Application.DTOs.Common;
using Gym.Application.DTOs.Payments;
using Gym.Application.Features.Payments.Commands.CreatePayment;
using Gym.Application.Features.Payments.Commands.CreateRazorpayOrder;
using Gym.Application.Features.Payments.Commands.GenerateInvoice;
using Gym.Application.Features.Payments.Commands.RefundPayment;
using Gym.Application.Features.Payments.Commands.VerifyRazorpayPayment;
using Gym.Application.Features.Payments.Queries.GetInvoiceById;
using Gym.Application.Features.Payments.Queries.GetMonthlyRevenue;
using Gym.Application.Features.Payments.Queries.GetPayments;
using Gym.Application.Features.Payments.Queries.GetPaymentsByMember;
using Gym.Application.Features.Payments.Queries.GetRazorpayCheckoutContext;
using Gym.Application.Features.Payments.Queries.GetRevenueDashboard;
using Gym.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gym.API.Controllers;

[ApiController]
[Route("api/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IInvoicePdfGenerator _pdfGenerator;

    public PaymentsController(IMediator mediator, IInvoicePdfGenerator pdfGenerator)
    {
        _mediator = mediator;
        _pdfGenerator = pdfGenerator;
    }

    [HttpGet]
    [RequirePermission(Permissions.ViewPayments)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PaymentResponseDto>>>> GetHistory(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var payments = await _mediator.Send(new GetPaymentsQuery(search), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PaymentResponseDto>>.Ok(payments));
    }

    [HttpGet("member/{memberId:int}")]
    [RequirePermission(Permissions.ViewPayments)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<PaymentResponseDto>>>> GetByMember(
        int memberId,
        CancellationToken cancellationToken)
    {
        var payments = await _mediator.Send(new GetPaymentsByMemberQuery(memberId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PaymentResponseDto>>.Ok(payments));
    }

    [HttpGet("revenue/dashboard")]
    [RequirePermission(Permissions.ViewRevenue)]
    public async Task<ActionResult<ApiResponse<RevenueDashboardDto>>> GetRevenueDashboard(
        [FromQuery] Guid? gymId,
        CancellationToken cancellationToken)
    {
        var dashboard = await _mediator.Send(new GetRevenueDashboardQuery(gymId), cancellationToken);
        return Ok(ApiResponse<RevenueDashboardDto>.Ok(dashboard));
    }

    [HttpGet("revenue/monthly")]
    [RequirePermission(Permissions.ViewRevenue)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<MonthlyRevenueDto>>>> GetMonthlyRevenue(
        [FromQuery] int months = 12,
        CancellationToken cancellationToken = default)
    {
        var data = await _mediator.Send(new GetMonthlyRevenueQuery(months), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<MonthlyRevenueDto>>.Ok(data));
    }

    [HttpPost]
    [RequirePermission(Permissions.CreatePayment)]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> Create(
        [FromBody] CreatePaymentDto dto,
        CancellationToken cancellationToken)
    {
        var payment = await _mediator.Send(new CreatePaymentCommand(dto), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<PaymentResponseDto>.Ok(payment, "Payment recorded."));
    }

    [HttpPost("{paymentId:int}/invoice")]
    [RequirePermission(Permissions.DownloadInvoice)]
    public async Task<ActionResult<ApiResponse<InvoiceDto>>> GenerateInvoice(
        int paymentId,
        CancellationToken cancellationToken)
    {
        var invoice = await _mediator.Send(new GenerateInvoiceCommand(paymentId), cancellationToken);
        return Ok(ApiResponse<InvoiceDto>.Ok(invoice, "Invoice generated."));
    }

    [HttpGet("invoices/{invoiceId:int}/download")]
    [RequirePermission(Permissions.DownloadInvoice)]
    public async Task<IActionResult> DownloadInvoice(int invoiceId, CancellationToken cancellationToken)
    {
        var invoice = await _mediator.Send(new GetInvoiceByIdQuery(invoiceId), cancellationToken);
        var bytes = _pdfGenerator.Generate(invoice);
        return File(bytes, "application/pdf", $"{invoice.InvoiceNumber}.pdf");
    }

    [HttpGet("razorpay/checkout-context")]
    [RequirePermission(Permissions.InitiateOnlinePayment)]
    public async Task<ActionResult<ApiResponse<RazorpayCheckoutContextDto>>> GetCheckoutContext(
        CancellationToken cancellationToken)
    {
        var context = await _mediator.Send(new GetRazorpayCheckoutContextQuery(), cancellationToken);
        if (context is null)
            return NotFound(ApiResponse<RazorpayCheckoutContextDto>.Fail("No payable membership found."));
        return Ok(ApiResponse<RazorpayCheckoutContextDto>.Ok(context));
    }

    [HttpPost("razorpay/order")]
    [RequirePermission(Permissions.InitiateOnlinePayment)]
    public async Task<ActionResult<ApiResponse<RazorpayOrderResponseDto>>> CreateRazorpayOrder(
        [FromBody] CreateRazorpayOrderDto dto,
        CancellationToken cancellationToken)
    {
        var order = await _mediator.Send(new CreateRazorpayOrderCommand(dto), cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<RazorpayOrderResponseDto>.Ok(order, "Razorpay order created."));
    }

    [HttpPost("razorpay/verify")]
    [RequirePermission(Permissions.InitiateOnlinePayment)]
    public async Task<ActionResult<ApiResponse<PaymentResponseDto>>> VerifyRazorpayPayment(
        [FromBody] VerifyRazorpayPaymentDto dto,
        CancellationToken cancellationToken)
    {
        var payment = await _mediator.Send(new VerifyRazorpayPaymentCommand(dto), cancellationToken);
        return Ok(ApiResponse<PaymentResponseDto>.Ok(payment, "Payment verified successfully."));
    }

    [HttpPost("{paymentId:int}/refund")]
    [RequirePermission(Permissions.RefundPayment)]
    public async Task<ActionResult<ApiResponse<RefundPaymentResponseDto>>> Refund(
        int paymentId,
        [FromBody] RefundPaymentDto dto,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new RefundPaymentCommand(paymentId, dto), cancellationToken);
        return Ok(ApiResponse<RefundPaymentResponseDto>.Ok(result, "Payment refunded."));
    }
}

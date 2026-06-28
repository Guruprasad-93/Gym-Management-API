# Creates a fresh SaaS payment order and opens the checkout harness in your browser.
# After paying, copy the JSON from the page (or save to razorpay-manual-payment.json) and run:
#   .\razorpay-post-manual-validate.ps1

param(
    [string]$BaseUrl = 'http://localhost:5088',
    [string]$GymId = '',
    [string]$LoginId = 'admin',
    [string]$Password = 'Demo@123',
    [int]$SaasPlanId = 2,
    [int]$PricingOptionId = 1,
    [string]$OrderJson = 'razorpay-manual-order.json'
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($GymId)) {
    $GymId = (sqlcmd -S . -d GymDb -E -h -1 -W -Q "SET NOCOUNT ON; SELECT TOP 1 CAST(GymId AS NVARCHAR(36)) FROM dbo.Gyms WHERE Name = N'FitZone Demo Gym';" |
        Where-Object { $_ -match '^[0-9A-Fa-f-]{36}$' } | Select-Object -First 1)
}

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$null = Invoke-WebRequest "$BaseUrl/api/auth/csrf" -WebSession $session -UseBasicParsing
$csrf = ($session.Cookies.GetCookies($BaseUrl) | Where-Object Name -eq 'XSRF-TOKEN').Value
$loginBody = @{ loginIdentifier = $LoginId; password = $Password; gymId = $GymId } | ConvertTo-Json
$null = Invoke-WebRequest "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType 'application/json' -WebSession $session -Headers @{ 'X-XSRF-TOKEN' = $csrf } -UseBasicParsing
$csrf = ($session.Cookies.GetCookies($BaseUrl) | Where-Object Name -eq 'XSRF-TOKEN').Value

$orderBody = @{ saasPlanId = $SaasPlanId; pricingOptionId = $PricingOptionId } | ConvertTo-Json
$orderResp = Invoke-RestMethod "$BaseUrl/api/saas/payments/order?gymId=$GymId" -Method POST -Body $orderBody -ContentType 'application/json' -WebSession $session -Headers @{ 'X-XSRF-TOKEN' = $csrf }
$order = $orderResp.data

$context = @{
    createdAt = (Get-Date).ToUniversalTime().ToString('o')
    baseUrl = $BaseUrl
    gymId = $GymId
    saasPaymentId = $order.saasPaymentId
    razorpayOrderId = $order.razorpayOrderId
    amount = $order.amount
    amountPaise = [int][Math]::Round($order.amount * 100)
    keyId = $order.keyId
    planName = $order.planName
    pricingOptionId = $order.pricingOptionId
}
$context | ConvertTo-Json -Depth 4 | Set-Content (Join-Path $PSScriptRoot $OrderJson)

$harness = Join-Path $PSScriptRoot 'razorpay-checkout-harness.html'
$q = [System.Web.HttpUtility]::UrlEncode
$url = "file:///$($harness.Replace('\','/'))?key=$($order.keyId)&order_id=$($order.razorpayOrderId)&amount=$($context.amountPaise)&currency=INR&name=FitZone%20Demo%20Gym&description=$([uri]::EscapeDataString($order.planName))&saas_payment_id=$($order.saasPaymentId)"

Write-Host ''
Write-Host '=== MANUAL CHECKOUT READY ===' -ForegroundColor Cyan
Write-Host "SaasPaymentId   : $($order.saasPaymentId)"
Write-Host "RazorpayOrderId : $($order.razorpayOrderId)"
Write-Host "Amount          : INR $($order.amount)"
Write-Host "Order saved     : $OrderJson"
Write-Host ''
Write-Host 'Opening checkout harness in default browser...' -ForegroundColor Green
Start-Process $url
Write-Host ''
Write-Host 'After payment success, run:' -ForegroundColor Yellow
Write-Host "  .\razorpay-post-manual-validate.ps1 -PaymentJson razorpay-manual-payment.json" -ForegroundColor White

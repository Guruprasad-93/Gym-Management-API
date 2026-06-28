# Validates steps 4-10 after a manual Razorpay Test Mode payment.
# Usage:
#   .\razorpay-post-manual-validate.ps1 -PaymentJson razorpay-manual-payment.json
#   .\razorpay-post-manual-validate.ps1 -SaasPaymentId 3 -RazorpayOrderId order_xxx -RazorpayPaymentId pay_xxx -RazorpaySignature sig_xxx

param(
    [string]$BaseUrl = 'http://localhost:5088',
    [string]$GymId = '',
    [string]$LoginId = 'admin',
    [string]$Password = 'Demo@123',
    [string]$SqlServer = '.',
    [string]$SqlDatabase = 'GymDb',
    [string]$OrderJson = 'razorpay-manual-order.json',
    [string]$PaymentJson = '',
    [int]$SaasPaymentId = 0,
    [string]$RazorpayOrderId = '',
    [string]$RazorpayPaymentId = '',
    [string]$RazorpaySignature = '',
    [string]$OutputJson = 'razorpay-manual-validation-results.json',
    [string]$ScreenshotDir = 'razorpay-e2e-screenshots'
)

$ErrorActionPreference = 'Stop'
$report = [ordered]@{}

function Invoke-SqlScalar([string]$Query) {
    $out = sqlcmd -S $SqlServer -d $SqlDatabase -E -h -1 -W -Q "SET NOCOUNT ON; $Query"
    return ($out | Where-Object { $_ -and $_.ToString().Trim() -ne '' } | Select-Object -First 1).ToString().Trim()
}

function Get-ErrBody($err) {
    try { return $err.ErrorDetails.Message } catch { return $err.Exception.Message }
}

if ($PaymentJson -and (Test-Path (Join-Path $PSScriptRoot $PaymentJson))) {
    $pay = Get-Content (Join-Path $PSScriptRoot $PaymentJson) -Raw | ConvertFrom-Json
    if (-not $SaasPaymentId -and $pay.saas_payment_id) { $SaasPaymentId = [int]$pay.saas_payment_id }
    if (-not $RazorpayOrderId) { $RazorpayOrderId = $pay.razorpay_order_id }
    if (-not $RazorpayPaymentId) { $RazorpayPaymentId = $pay.razorpay_payment_id }
    if (-not $RazorpaySignature) { $RazorpaySignature = $pay.razorpay_signature }
}

if ((Test-Path (Join-Path $PSScriptRoot $OrderJson)) -and -not $GymId) {
    $orderCtx = Get-Content (Join-Path $PSScriptRoot $OrderJson) -Raw | ConvertFrom-Json
    if (-not $GymId) { $GymId = $orderCtx.gymId }
    if (-not $SaasPaymentId) { $SaasPaymentId = [int]$orderCtx.saasPaymentId }
    if (-not $RazorpayOrderId) { $RazorpayOrderId = $orderCtx.razorpayOrderId }
}

if ([string]::IsNullOrWhiteSpace($GymId)) {
    $GymId = Invoke-SqlScalar "SELECT TOP 1 CAST(GymId AS NVARCHAR(36)) FROM dbo.Gyms WHERE Name = N'FitZone Demo Gym';"
}

if (-not $SaasPaymentId -or -not $RazorpayOrderId -or -not $RazorpayPaymentId -or -not $RazorpaySignature) {
    throw 'Missing payment fields. Provide -PaymentJson or -SaasPaymentId -RazorpayOrderId -RazorpayPaymentId -RazorpaySignature'
}

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession
$null = Invoke-WebRequest "$BaseUrl/api/auth/csrf" -WebSession $session -UseBasicParsing
$csrf = ($session.Cookies.GetCookies($BaseUrl) | Where-Object Name -eq 'XSRF-TOKEN').Value
$loginBody = @{ loginIdentifier = $LoginId; password = $Password; gymId = $GymId } | ConvertTo-Json
$null = Invoke-WebRequest "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType 'application/json' -WebSession $session -Headers @{ 'X-XSRF-TOKEN' = $csrf } -UseBasicParsing
$csrf = ($session.Cookies.GetCookies($BaseUrl) | Where-Object Name -eq 'XSRF-TOKEN').Value

function Api($Method, $Path, $Body = $null) {
    $params = @{ Uri = "$BaseUrl$Path"; Method = $Method; WebSession = $session; ContentType = 'application/json' }
    if ($Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 8 -Compress)
        $params.Headers = @{ 'X-XSRF-TOKEN' = ($session.Cookies.GetCookies($BaseUrl) | Where-Object Name -eq 'XSRF-TOKEN').Value }
    }
    try {
        $r = Invoke-RestMethod @params
        return @{ Ok = $true; Status = 200; Data = $r }
    }
    catch {
        $code = 0
        try { $code = [int]$_.Exception.Response.StatusCode } catch {}
        return @{ Ok = $false; Status = $code; Body = (Get-ErrBody $_) }
    }
}

$subBefore = Invoke-SqlScalar @"
SELECT CONCAT(SaasPlanId,'|',ISNULL(CAST(PricingOptionId AS NVARCHAR(20)),'null'),'|',CONVERT(NVARCHAR(30),CurrentPeriodEnd,126),'|',CONVERT(NVARCHAR(30),GraceEndsAt,126))
FROM dbo.GymSubscriptions WHERE GymId = '$GymId'
  AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');
"@
$sessionBefore = Api GET '/api/auth/session'

# Step 4 — payment captured (manual)
$step4Pass = $RazorpayPaymentId -like 'pay_*' -and $RazorpayOrderId -like 'order_*' -and $RazorpaySignature.Length -gt 10
$report['4_PaymentSuccess'] = @{
    Status = if ($step4Pass) { 'PASS' } else { 'FAIL' }
    Details = @{
        razorpayPaymentId = $RazorpayPaymentId
        razorpayOrderId = $RazorpayOrderId
        razorpaySignature = $RazorpaySignature.Substring(0, [Math]::Min(16, $RazorpaySignature.Length)) + '...'
        saasPaymentId = $SaasPaymentId
    }
}

# Step 5 — verify
$verifyBody = @{
    saasPaymentId = $SaasPaymentId
    razorpayOrderId = $RazorpayOrderId
    razorpayPaymentId = $RazorpayPaymentId
    razorpaySignature = $RazorpaySignature
}
$verifyResp = Api POST "/api/saas/payments/verify?gymId=$GymId" $verifyBody
$verified = $verifyResp.Data.data
$payStatus = Invoke-SqlScalar "SELECT Status FROM dbo.SaasSubscriptionPayments WHERE SaasPaymentId = $SaasPaymentId;"
$subAfter = Invoke-SqlScalar @"
SELECT CONCAT(SaasPlanId,'|',ISNULL(CAST(PricingOptionId AS NVARCHAR(20)),'null'),'|',CONVERT(NVARCHAR(30),CurrentPeriodEnd,126),'|',CONVERT(NVARCHAR(30),GraceEndsAt,126))
FROM dbo.GymSubscriptions WHERE GymId = '$GymId'
  AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');
"@
$step5Pass = $verifyResp.Ok -and ($payStatus -eq 'Completed') -and ($subBefore -ne $subAfter)
$report['5_PaymentVerify'] = @{
    Status = if ($step5Pass) { 'PASS' } else { 'FAIL' }
    Details = @{
        httpStatus = $verifyResp.Status
        verifyResponse = $verifyResp.Data
        paymentStatus = $payStatus
        subscriptionBefore = $subBefore
        subscriptionAfter = $subAfter
        currentPeriodEnd = $verified.endDate
        graceEndsAt = Invoke-SqlScalar "SELECT CONVERT(NVARCHAR(30),GraceEndsAt,126) FROM dbo.GymSubscriptions WHERE GymId = '$GymId' AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');"
    }
}

# Step 6 — session refresh (no re-login)
$sessionAfter = Api GET '/api/auth/session'
$sess = $sessionAfter.Data.data
$step6Pass = $sessionAfter.Ok -and $sess.enabledFeatureCodes -and ($null -ne $sess.enabledMenuCodes)
$report['6_SessionRefresh'] = @{
    Status = if ($step6Pass) { 'PASS' } else { 'FAIL' }
    Details = @{
        enabledFeatureCodes = $sess.enabledFeatureCodes
        enabledMenuCodes = $sess.enabledMenuCodes
        showPoweredBy = $sess.showPoweredBy
        featuresBefore = $sessionBefore.Data.data.enabledFeatureCodes
        featuresAfter = $sess.enabledFeatureCodes
    }
}

# Step 7 — duplicate verify
$endBeforeDup = Invoke-SqlScalar "SELECT CONVERT(NVARCHAR(30),CurrentPeriodEnd,126) FROM dbo.GymSubscriptions WHERE GymId = '$GymId' AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');"
$dupResp = Api POST "/api/saas/payments/verify?gymId=$GymId" $verifyBody
$endAfterDup = Invoke-SqlScalar "SELECT CONVERT(NVARCHAR(30),CurrentPeriodEnd,126) FROM dbo.GymSubscriptions WHERE GymId = '$GymId' AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');"
$completedCount = Invoke-SqlScalar "SELECT COUNT(*) FROM dbo.SaasSubscriptionPayments WHERE SaasPaymentId = $SaasPaymentId AND Status = N'Completed';"
$step7Pass = $dupResp.Ok -and ($endBeforeDup -eq $endAfterDup) -and ($completedCount -eq '1')
$report['7_DuplicateVerify'] = @{
    Status = if ($step7Pass) { 'PASS' } else { 'FAIL' }
    Details = @{
        secondVerifyOk = $dupResp.Ok
        endDateUnchanged = ($endBeforeDup -eq $endAfterDup)
        completedPaymentRows = $completedCount
    }
}

# Step 8 — database
$paymentRows = Invoke-SqlScalar "SELECT COUNT(*) FROM dbo.SaasSubscriptionPayments WHERE SaasPaymentId = $SaasPaymentId;"
$completedForOrder = Invoke-SqlScalar "SELECT COUNT(*) FROM dbo.SaasSubscriptionPayments WHERE RazorpayOrderId = N'$RazorpayOrderId' AND Status = N'Completed';"
$step8Pass = ($paymentRows -eq '1') -and ($completedForOrder -eq '1') -and ($payStatus -eq 'Completed')
$report['8_DatabaseValidation'] = @{
    Status = if ($step8Pass) { 'PASS' } else { 'FAIL' }
    Details = @{
        paymentRowsForId = $paymentRows
        completedForOrder = $completedForOrder
        paymentStatus = $payStatus
        gymSubscriptionRow = $subAfter
    }
}

# Step 9 — screenshots (manual checklist)
$shotDir = Join-Path $PSScriptRoot $ScreenshotDir
$shots = @(
    @{ name = 'PaymentSuccess'; path = (Join-Path $shotDir '04-payment-success.png'); exists = (Test-Path (Join-Path $shotDir '04-payment-success.png')) }
    @{ name = 'SubscriptionOverview'; path = (Join-Path $shotDir '05-subscription-overview.png'); exists = (Test-Path (Join-Path $shotDir '05-subscription-overview.png')) }
    @{ name = 'UpdatedSidebar'; path = (Join-Path $shotDir '06-updated-sidebar.png'); exists = (Test-Path (Join-Path $shotDir '06-updated-sidebar.png')) }
)
$step9Pass = ($shots | Where-Object exists).Count -eq 3
$report['9_Screenshots'] = @{ Status = if ($step9Pass) { 'PASS' } else { 'PARTIAL' }; Details = $shots }

$apiSteps = @($step4Pass, $step5Pass, $step6Pass, $step7Pass, $step8Pass)
$allPass = ($apiSteps -notcontains $false) -and $step9Pass

$final = [ordered]@{
    generatedAt = (Get-Date).ToUniversalTime().ToString('o')
    gymId = $GymId
    summary = @{
        '4_PaymentSuccess' = if ($step4Pass) { 'PASS' } else { 'FAIL' }
        '5_PaymentVerify' = if ($step5Pass) { 'PASS' } else { 'FAIL' }
        '6_SessionRefresh' = if ($step6Pass) { 'PASS' } else { 'FAIL' }
        '7_DuplicateVerify' = if ($step7Pass) { 'PASS' } else { 'FAIL' }
        '8_DatabaseValidation' = if ($step8Pass) { 'PASS' } else { 'FAIL' }
        '9_Screenshots' = if ($step9Pass) { 'PASS' } else { 'PARTIAL' }
        '10_FinalResult' = if ($allPass) { 'PASS' } else { 'FAIL' }
    }
    steps = $report
}

$outPath = Join-Path $PSScriptRoot $OutputJson
$final | ConvertTo-Json -Depth 10 | Set-Content $outPath

Write-Host ''
Write-Host '=== POST-PAYMENT VALIDATION ===' -ForegroundColor Cyan
$final.summary.GetEnumerator() | ForEach-Object { Write-Host "$($_.Key): $($_.Value)" }
Write-Host "Report: $outPath" -ForegroundColor DarkGray

if ($allPass) {
    Write-Host ''
    Write-Host 'RAZORPAY TEST MODE PAYMENT COMPLETED SUCCESSFULLY' -ForegroundColor Green
    Write-Host 'Production: switch User Secrets / env to rzp_live_* after deployment + one live smoke payment.' -ForegroundColor Green
    exit 0
}
exit 1

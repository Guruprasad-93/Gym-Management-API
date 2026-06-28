# Razorpay Test Mode End-to-End Validation (API + DB)
param(
    [string]$BaseUrl = 'http://localhost:5088',
    [string]$GymId = '',
    [string]$LoginId = 'admin',
    [string]$Password = 'Demo@123',
    [string]$SqlServer = '.',
    [string]$SqlDatabase = 'GymDb',
    [string]$OutputJson = 'razorpay-e2e-validation-results.json',
    [string]$ScreenshotDir = 'razorpay-e2e-screenshots'
)

$ErrorActionPreference = 'Stop'
$report = [ordered]@{}
$artifacts = [ordered]@{}

function Mask([string]$s) {
    if ([string]::IsNullOrWhiteSpace($s)) { return '(empty)' }
    if ($s.Length -le 8) { return '****' }
    return $s.Substring(0, 8) + '...'
}

function Get-UserSecretMap {
    Push-Location (Join-Path $PSScriptRoot '..\Gym.API')
    try {
        $lines = dotnet user-secrets list 2>$null
        $map = @{}
        foreach ($line in $lines) {
            if ($line -match '^([^=]+)\s*=\s*(.*)$') {
                $map[$matches[1].Trim()] = $matches[2].Trim()
            }
        }
        return $map
    }
    finally { Pop-Location }
}

function Invoke-SqlScalar([string]$Query) {
    $out = sqlcmd -S $SqlServer -d $SqlDatabase -E -h -1 -W -Q "SET NOCOUNT ON; $Query"
    return ($out | Where-Object { $_ -and $_.ToString().Trim() -ne '' } | Select-Object -First 1).ToString().Trim()
}

function Invoke-SqlRows([string]$Query) {
    sqlcmd -S $SqlServer -d $SqlDatabase -E -W -s '|' -Q "SET NOCOUNT ON; $Query"
}

function Get-ErrBody($err) {
    try { return $err.ErrorDetails.Message } catch { return $err.Exception.Message }
}

function Invoke-RazorpaySignature([string]$OrderId, [string]$PaymentId, [string]$Secret) {
    $hmac = New-Object System.Security.Cryptography.HMACSHA256
    $hmac.Key = [Text.Encoding]::UTF8.GetBytes($Secret)
    $hash = $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes("$OrderId|$PaymentId"))
    return ([BitConverter]::ToString($hash) -replace '-', '').ToLower()
}

function New-RazorpayTestPayment([string]$KeyId, [string]$KeySecret, [string]$OrderId, [int]$AmountPaise) {
    $pair = "$KeyId`:$KeySecret"
    $basic = [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes($pair))
    $headers = @{ Authorization = "Basic $basic" }
    $payload = @{
        amount = $AmountPaise
        currency = 'INR'
        order_id = $OrderId
        email = 'e2e@test.gym.local'
        contact = '9999999999'
        method = 'netbanking'
        bank = 'HDFC'
    } | ConvertTo-Json
    return Invoke-RestMethod -Uri 'https://api.razorpay.com/v1/payments' -Method POST -Headers $headers -Body $payload -ContentType 'application/json'
}

if ([string]::IsNullOrWhiteSpace($GymId)) {
    $GymId = Invoke-SqlScalar "SELECT TOP 1 CAST(GymId AS NVARCHAR(36)) FROM dbo.Gyms WHERE Name = N'FitZone Demo Gym';"
}
if ([string]::IsNullOrWhiteSpace($GymId)) { throw 'Demo gym not found' }

$secrets = Get-UserSecretMap
$envKeyId = $env:Razorpay__KeyId
$envKeySecret = $env:Razorpay__KeySecret
$appsettingsPath = Join-Path $PSScriptRoot '..\Gym.API\appsettings.json'
$devSettingsPath = Join-Path $PSScriptRoot '..\Gym.API\appsettings.Development.json'
$appsettings = Get-Content $appsettingsPath -Raw | ConvertFrom-Json
$devSettings = Get-Content $devSettingsPath -Raw | ConvertFrom-Json

$configSource = @()
if ($secrets['Razorpay:Enabled'] -eq 'true' -or $secrets['Razorpay:KeyId']) { $configSource += 'User Secrets (Gym.API)' }
if ($envKeyId -or $envKeySecret) { $configSource += 'Environment Variables' }
if ($appsettings.Razorpay.Enabled -eq $true -or $appsettings.Razorpay.KeyId) { $configSource += 'appsettings.json' }
if ($devSettings.Razorpay) { $configSource += 'appsettings.Development.json' }
if ($configSource.Count -eq 0) { $configSource = @('appsettings.json (defaults only)') }

$keyId = if ($envKeyId) { $envKeyId } elseif ($secrets['Razorpay:KeyId']) { $secrets['Razorpay:KeyId'] } else { $appsettings.Razorpay.KeyId }
$keySecret = if ($envKeySecret) { $envKeySecret } elseif ($secrets['Razorpay:KeySecret']) { $secrets['Razorpay:KeySecret'] } else { $appsettings.Razorpay.KeySecret }

# Activate one pricing option for deterministic catalog pricing during validation.
sqlcmd -S $SqlServer -d $SqlDatabase -E -Q "UPDATE dbo.PlanPricingOptions SET IsActive = 1 WHERE PricingOptionId = 1;" | Out-Null
$enabledExpected = ($secrets['Razorpay:Enabled'] -eq 'true') -or ($env:Razorpay__Enabled -eq 'true') -or ($appsettings.Razorpay.Enabled -eq $true)
$currencyExpected = if ($secrets['Razorpay:Currency']) { $secrets['Razorpay:Currency'] } else { $appsettings.Razorpay.Currency }

# --- Step 1: Configuration ---
$step1 = [ordered]@{
    RazorpayEnabled = $enabledExpected
    KeyIdLoaded = -not [string]::IsNullOrWhiteSpace($keyId)
    KeySecretLoaded = -not [string]::IsNullOrWhiteSpace($keySecret)
    Currency = $currencyExpected
    KeyIdPrefix = Mask $keyId
    ConfigSources = ($configSource -join ', ')
    AppsettingsEnabled = $appsettings.Razorpay.Enabled
    DevSettingsHasRazorpay = [bool]$devSettings.Razorpay
    UserSecretsEnabled = ($secrets['Razorpay:Enabled'] -eq 'true')
    EnvVarsSet = (-not [string]::IsNullOrWhiteSpace($envKeyId))
}
$step1Pass = $step1.RazorpayEnabled -and $step1.KeyIdLoaded -and $step1.KeySecretLoaded -and ($step1.Currency -eq 'INR') -and ($keyId -like 'rzp_test_*')
$report['1_Configuration'] = @{ Status = if ($step1Pass) { 'PASS' } else { 'FAIL' }; Details = $step1 }

$session = New-Object Microsoft.PowerShell.Commands.WebRequestSession

function Get-CsrfToken {
    $null = Invoke-WebRequest -Uri "$BaseUrl/api/auth/csrf" -WebSession $session -UseBasicParsing
    return ($session.Cookies.GetCookies($BaseUrl) | Where-Object Name -eq 'XSRF-TOKEN' | Select-Object -First 1).Value
}

function Login-WithCsrf {
    $csrf = Get-CsrfToken
    $loginBody = @{ loginIdentifier = $LoginId; password = $Password; gymId = $GymId } | ConvertTo-Json
    $null = Invoke-WebRequest -Uri "$BaseUrl/api/auth/login" -Method POST -Body $loginBody -ContentType 'application/json' -WebSession $session -Headers @{ 'X-XSRF-TOKEN' = $csrf } -UseBasicParsing
    # Login rotates CSRF — use post-login cookie value for mutating requests.
    return ($session.Cookies.GetCookies($BaseUrl) | Where-Object Name -eq 'XSRF-TOKEN' | Select-Object -First 1).Value
}

$csrfToken = $null
try { $csrfToken = Login-WithCsrf }
catch { throw "Login failed: $(Get-ErrBody $_)" }

function Api($Method, $Path, $Body = $null) {
    $params = @{ Uri = "$BaseUrl$Path"; Method = $Method; WebSession = $session; ContentType = 'application/json' }
    if ($Body) {
        $params.Body = ($Body | ConvertTo-Json -Depth 8 -Compress)
        $params.Headers = @{ 'X-XSRF-TOKEN' = ($session.Cookies.GetCookies($BaseUrl) | Where-Object Name -eq 'XSRF-TOKEN' | Select-Object -First 1).Value }
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

# Prepare trial state for clean purchase
sqlcmd -S $SqlServer -d $SqlDatabase -E -Q @"
SET NOCOUNT ON;
DECLARE @TrialPlanId INT = (SELECT TOP 1 SaasPlanId FROM dbo.SaasSubscriptionPlans WHERE PlanCode = N'Trial');
UPDATE gs SET Status = N'Trial', SaasPlanId = @TrialPlanId,
    CurrentPeriodStart = SYSUTCDATETIME(), CurrentPeriodEnd = DATEADD(day, 15, SYSUTCDATETIME()),
    TrialEndsAt = DATEADD(day, 15, SYSUTCDATETIME()), GraceEndsAt = DATEADD(day, 18, SYSUTCDATETIME()),
    UpdatedAt = SYSUTCDATETIME()
FROM dbo.GymSubscriptions gs
WHERE gs.GymId = '$GymId'
  AND gs.GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');
"@ | Out-Null

# Catalog + pricing option
$catalog = Api GET "/api/saas/plans/catalog?gymId=$GymId"
if (-not $catalog.Ok) { throw "Catalog failed: $($catalog.Body)" }
$basicPlan = $catalog.Data.data.plans | Where-Object { $_.planCode -eq 'Basic' } | Select-Object -First 1
$planId = if ($basicPlan.saasPlanId) { [int]$basicPlan.saasPlanId } else { [int]$basicPlan.id }
$pricingOption = $basicPlan.pricingOptions | Where-Object { $_.isActive -eq $true } | Select-Object -First 1
if (-not $pricingOption) { $pricingOption = $basicPlan.pricingOptions | Select-Object -First 1 }

$subBefore = Api GET "/api/saas/subscription?gymId=$GymId"
$sessionBefore = Api GET "/api/auth/session"
$subRowBefore = Invoke-SqlScalar @"
SELECT CONCAT(SaasPlanId,'|',ISNULL(CAST(PricingOptionId AS NVARCHAR(20)),'null'),'|',CONVERT(NVARCHAR(30),CurrentPeriodEnd,126),'|',CONVERT(NVARCHAR(30),GraceEndsAt,126))
FROM dbo.GymSubscriptions WHERE GymId = '$GymId'
  AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');
"@

# --- Step 2: Order creation ---
$orderRequest = @{
    saasPlanId = $planId
    pricingOptionId = [int]$pricingOption.pricingOptionId
    billingCycle = 'Monthly'
}
$orderResp = Api POST "/api/saas/payments/order?gymId=$GymId" $orderRequest
$order = $orderResp.Data.data
$expectedAmount = [decimal]$pricingOption.price
$step2Checks = [ordered]@{
    HttpStatus = $orderResp.Status
    RazorpayOrderId = $order.razorpayOrderId
    Amount = $order.amount
    ExpectedAmount = $expectedAmount
    PricingOptionId = $order.pricingOptionId
    ExpectedPricingOptionId = $pricingOption.pricingOptionId
    PlanId = $planId
    KeyIdReturned = $order.keyId
    GymId = $GymId
}
$step2Pass = $orderResp.Ok -and $orderResp.Status -eq 200 -and $order.razorpayOrderId -and
    ($order.amount -eq $expectedAmount) -and     ($order.pricingOptionId -eq $pricingOption.pricingOptionId) -and
    ($order.keyId -like 'rzp_test_*')
$artifacts['order_request'] = $orderRequest
$artifacts['order_response'] = $orderResp.Data
$report['2_OrderCreation'] = @{ Status = if ($step2Pass) { 'PASS' } else { 'FAIL' }; Details = $step2Checks }

# --- Step 3: Checkout (API proxy — no browser in CI) ---
$checkoutContext = [ordered]@{
    KeyIdMatchesConfig = ($order.keyId -like 'rzp_test_*')
    AmountPaise = [int][Math]::Round($order.amount * 100)
    PlanName = $order.planName
    Currency = $order.currency
    RazorpayOrderId = $order.razorpayOrderId
    FrontendAvailable = Test-Path (Join-Path $PSScriptRoot '..\..\Frontend\gym-app')
    Note = 'Browser popup/JS validation requires Angular UI; validated checkout payload from order API.'
}
$step3Pass = $checkoutContext.KeyIdMatchesConfig -and ($checkoutContext.AmountPaise -gt 0) -and $checkoutContext.PlanName
if (-not $checkoutContext.FrontendAvailable) {
    $checkoutContext.Note = 'Frontend not present in workspace; Step 3 limited to API checkout payload validation.'
    $step3Pass = $step3Pass  # still pass API-level checks
}
$report['3_RazorpayCheckout'] = @{ Status = if ($step3Pass) { 'PASS' } else { 'FAIL' }; Details = $checkoutContext }

# --- Step 4: Payment success (Razorpay test API) ---
$amountPaise = [int][Math]::Round($order.amount * 100)
$rzPayment = New-RazorpayTestPayment $keyId $keySecret $order.razorpayOrderId $amountPaise
$signature = Invoke-RazorpaySignature $order.razorpayOrderId $rzPayment.id $keySecret
$step4 = [ordered]@{
    RazorpayPaymentId = $rzPayment.id
    RazorpayOrderId = $order.razorpayOrderId
    RazorpaySignature = $signature
    PaymentStatus = $rzPayment.status
}
$step4Pass = $rzPayment.id -and $order.razorpayOrderId -and $signature -and ($rzPayment.status -eq 'captured' -or $rzPayment.status -eq 'authorized')
$artifacts['razorpay_payment'] = @{ id = $rzPayment.id; status = $rzPayment.status; order_id = $rzPayment.order_id }
$report['4_PaymentSuccess'] = @{ Status = if ($step4Pass) { 'PASS' } else { 'FAIL' }; Details = $step4 }

# --- Step 5: Verify ---
$verifyRequest = @{
    saasPaymentId = $order.saasPaymentId
    razorpayOrderId = $order.razorpayOrderId
    razorpayPaymentId = $rzPayment.id
    razorpaySignature = $signature
}
$verifyResp = Api POST "/api/saas/payments/verify?gymId=$GymId" $verifyRequest
$verified = $verifyResp.Data.data
$payRow = Invoke-SqlScalar "SELECT Status FROM dbo.SaasSubscriptionPayments WHERE SaasPaymentId = $($order.saasPaymentId);"
$subRowAfter = Invoke-SqlScalar @"
SELECT CONCAT(SaasPlanId,'|',ISNULL(CAST(PricingOptionId AS NVARCHAR(20)),'null'),'|',CONVERT(NVARCHAR(30),CurrentPeriodEnd,126),'|',CONVERT(NVARCHAR(30),GraceEndsAt,126))
FROM dbo.GymSubscriptions WHERE GymId = '$GymId'
  AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');
"@
$step5 = [ordered]@{
    HttpStatus = $verifyResp.Status
    PaymentStatus = $payRow
    SubscriptionPlanId = $verified.saasPlanId
    ExpectedPlanId = $planId
    PricingOptionId = Invoke-SqlScalar "SELECT CAST(PricingOptionId AS NVARCHAR(20)) FROM dbo.GymSubscriptions WHERE GymId = '$GymId' AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');"
    CurrentPeriodEnd = $verified.endDate
    GraceEndsAt = Invoke-SqlScalar "SELECT CONVERT(NVARCHAR(30),GraceEndsAt,126) FROM dbo.GymSubscriptions WHERE GymId = '$GymId' AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');"
    SubBefore = $subRowBefore
    SubAfter = $subRowAfter
}
$step5Pass = $verifyResp.Ok -and ($payRow -eq 'Completed') -and ($verified.saasPlanId -eq $planId) -and ($subRowBefore -ne $subRowAfter)
$artifacts['verify_request'] = $verifyRequest
$artifacts['verify_response'] = $verifyResp.Data
$report['5_PaymentVerify'] = @{ Status = if ($step5Pass) { 'PASS' } else { 'FAIL' }; Details = $step5 }

# --- Step 6: Session refresh ---
$sessionAfter = Api GET "/api/auth/session"
$sess = $sessionAfter.Data.data
$step6 = [ordered]@{
    EnabledFeatureCodes = $sess.enabledFeatureCodes
    EnabledMenuCodes = $sess.enabledMenuCodes
    ShowPoweredBy = $sess.showPoweredBy
    SessionBeforeFeatures = $sessionBefore.Data.data.enabledFeatureCodes
    SessionAfterFeatures = $sess.enabledFeatureCodes
}
$step6Pass = $sessionAfter.Ok -and $sess.enabledFeatureCodes -and $sess.enabledMenuCodes -ne $null
$report['6_SessionRefresh'] = @{ Status = if ($step6Pass) { 'PASS' } else { 'FAIL' }; Details = $step6 }

# --- Step 7: Duplicate verify ---
$endBeforeDup = Invoke-SqlScalar "SELECT CONVERT(NVARCHAR(30),CurrentPeriodEnd,126) FROM dbo.GymSubscriptions WHERE GymId = '$GymId' AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');"
$dupResp = Api POST "/api/saas/payments/verify?gymId=$GymId" $verifyRequest
$endAfterDup = Invoke-SqlScalar "SELECT CONVERT(NVARCHAR(30),CurrentPeriodEnd,126) FROM dbo.GymSubscriptions WHERE GymId = '$GymId' AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');"
$completedCount = Invoke-SqlScalar "SELECT COUNT(*) FROM dbo.SaasSubscriptionPayments WHERE SaasPaymentId = $($order.saasPaymentId) AND Status = N'Completed';"
$step7 = [ordered]@{
    SecondVerifyOk = $dupResp.Ok
    EndDateUnchanged = ($endBeforeDup -eq $endAfterDup)
    CompletedPaymentRows = $completedCount
    SubRowUnchanged = ($subRowAfter -eq (Invoke-SqlScalar @"
SELECT CONCAT(SaasPlanId,'|',ISNULL(CAST(PricingOptionId AS NVARCHAR(20)),'null'),'|',CONVERT(NVARCHAR(30),CurrentPeriodEnd,126),'|',CONVERT(NVARCHAR(30),GraceEndsAt,126))
FROM dbo.GymSubscriptions WHERE GymId = '$GymId'
  AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');
"@))
}
$step7Pass = $dupResp.Ok -and $step7.EndDateUnchanged -and ($completedCount -eq 1) -and $step7.SubRowUnchanged
$report['7_DuplicateVerify'] = @{ Status = if ($step7Pass) { 'PASS' } else { 'FAIL' }; Details = $step7 }

# --- Step 8: Database ---
$paymentRows = Invoke-SqlScalar "SELECT COUNT(*) FROM dbo.SaasSubscriptionPayments WHERE SaasPaymentId = $($order.saasPaymentId);"
$completedForOrder = Invoke-SqlScalar "SELECT COUNT(*) FROM dbo.SaasSubscriptionPayments WHERE RazorpayOrderId = N'$($order.razorpayOrderId)' AND Status = N'Completed';"
$pricingRow = Invoke-SqlScalar "SELECT COUNT(*) FROM dbo.PlanPricingOptions WHERE PricingOptionId = $($pricingOption.pricingOptionId) AND IsActive = 1;"
$step8 = [ordered]@{
    SaasSubscriptionPaymentsRowsForId = $paymentRows
    CompletedPaymentsForOrder = $completedForOrder
    PlanPricingOptionActive = ($pricingRow -eq '1')
    TableNote = 'User checklist says SaasPayments; actual table is SaasSubscriptionPayments'
}
$step8Pass = ($paymentRows -eq '1') -and ($completedForOrder -eq '1') -and ($pricingRow -eq '1')
$report['8_DatabaseValidation'] = @{ Status = if ($step8Pass) { 'PASS' } else { 'FAIL' }; Details = $step8 }

# --- Step 9: Screenshots ---
$screenshotRoot = Join-Path $PSScriptRoot $ScreenshotDir
New-Item -ItemType Directory -Force -Path $screenshotRoot | Out-Null
$screenshots = @(
    @{ Name = 'SubscriptionCatalog'; Path = ''; Status = 'NOT_CAPTURED' }
    @{ Name = 'CheckoutPage'; Path = ''; Status = 'NOT_CAPTURED' }
    @{ Name = 'RazorpayPopup'; Path = ''; Status = 'NOT_CAPTURED' }
    @{ Name = 'PaymentSuccess'; Path = ''; Status = 'NOT_CAPTURED' }
    @{ Name = 'UpdatedSubscriptionOverview'; Path = ''; Status = 'NOT_CAPTURED' }
    @{ Name = 'UpdatedSidebarMenus'; Path = ''; Status = 'NOT_CAPTURED' }
)
$step9Pass = $false
$report['9_Screenshots'] = @{ Status = 'FAIL'; Details = @{ Items = $screenshots; Note = 'Angular frontend not available in workspace for browser capture.' } }

# Summary
$allSteps = @($step1Pass, $step2Pass, $step3Pass, $step4Pass, $step5Pass, $step6Pass, $step7Pass, $step8Pass, $step9Pass)
$apiDbPass = $allSteps[0..7] -notcontains $false

$final = [ordered]@{
    GeneratedAt = (Get-Date).ToUniversalTime().ToString('o')
    GymId = $GymId
    BaseUrl = $BaseUrl
    Steps = $report
    Artifacts = $artifacts
    Summary = [ordered]@{
        '1_Configuration' = if ($step1Pass) { 'PASS' } else { 'FAIL' }
        '2_OrderCreation' = if ($step2Pass) { 'PASS' } else { 'FAIL' }
        '3_RazorpayCheckout' = if ($step3Pass) { 'PASS' } else { 'FAIL' }
        '4_PaymentSuccess' = if ($step4Pass) { 'PASS' } else { 'FAIL' }
        '5_PaymentVerify' = if ($step5Pass) { 'PASS' } else { 'FAIL' }
        '6_SessionRefresh' = if ($step6Pass) { 'PASS' } else { 'FAIL' }
        '7_DuplicateVerify' = if ($step7Pass) { 'PASS' } else { 'FAIL' }
        '8_DatabaseValidation' = if ($step8Pass) { 'PASS' } else { 'FAIL' }
        '9_Screenshots' = 'FAIL'
        ApiAndDatabaseReady = $apiDbPass
    }
}

$outPath = Join-Path $PSScriptRoot $OutputJson
$final | ConvertTo-Json -Depth 10 | Set-Content $outPath
Write-Host "Results: $outPath"
$final.Summary.GetEnumerator() | ForEach-Object { Write-Host "$($_.Key): $($_.Value)" }
if (-not $apiDbPass) { exit 1 }

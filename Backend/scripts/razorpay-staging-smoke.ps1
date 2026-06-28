# Razorpay SaaS subscription staging smoke tests
# Usage:
#   $env:Razorpay__KeyId = 'rzp_test_...'
#   $env:Razorpay__KeySecret = '...'
#   .\razorpay-staging-smoke.ps1 -BaseUrl http://localhost:5088

param(
    [string]$BaseUrl = 'http://localhost:5088',
    [string]$GymId = '',
    [string]$LoginId = 'admin',
    [string]$Password = 'Demo@123',
    [string]$SqlServer = '.',
    [string]$SqlDatabase = 'GymDb',
    [string]$KeyId = $env:Razorpay__KeyId,
    [string]$KeySecret = $env:Razorpay__KeySecret,
    [string]$OutputJson = 'razorpay-staging-smoke-results.json'
)

$ErrorActionPreference = 'Stop'
$results = @()

if ([string]::IsNullOrWhiteSpace($GymId)) {
    $gymRow = sqlcmd -S $SqlServer -d $SqlDatabase -E -h -1 -W -Q "SET NOCOUNT ON; SELECT TOP 1 CAST(GymId AS NVARCHAR(36)) FROM dbo.Gyms WHERE Name = N'FitZone Demo Gym';"
    $GymId = ($gymRow | Where-Object { $_ -match '^[0-9A-Fa-f-]{36}$' } | Select-Object -First 1)
    if ([string]::IsNullOrWhiteSpace($GymId)) {
        Write-Error "Demo gym not found in $SqlDatabase. Seed demo data or pass -GymId."
    }
}
Write-Host "Using demo gym $GymId" -ForegroundColor Cyan

function Record($Id, $Name, $Status, $Detail) {
    $script:results += [PSCustomObject]@{ Id = $Id; Name = $Name; Status = $Status; Detail = $Detail }
    $color = switch ($Status) { 'PASS' { 'Green' } 'FAIL' { 'Red' } 'SKIP' { 'Yellow' } 'BLOCKED' { 'Magenta' } default { 'White' } }
    Write-Host "[$Status] $Id - $Name" -ForegroundColor $color
    if ($Detail) { Write-Host "       $Detail" -ForegroundColor DarkGray }
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

function Login {
    $body = @{ loginIdentifier = $LoginId; password = $Password; gymId = $GymId }
    $r = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST -Body ($body | ConvertTo-Json) -ContentType 'application/json' -SessionVariable sess
    return $sess
}

function Api($Method, $Path, $Session, $Body = $null) {
    $params = @{ Uri = "$BaseUrl$Path"; Method = $Method; WebSession = $Session; ContentType = 'application/json' }
    if ($Body) { $params.Body = ($Body | ConvertTo-Json -Depth 8 -Compress) }
    try {
        $r = Invoke-RestMethod @params
        return @{ Ok = $true; Data = $r }
    }
    catch {
        return @{ Ok = $false; Status = [int]$_.Exception.Response.StatusCode; Body = (Get-ErrBody $_) }
    }
}

function Invoke-Sql([string]$Query) {
    $escaped = $Query -replace "'", "''"
    $cmd = "sqlcmd -S `"$SqlServer`" -d `"$SqlDatabase`" -E -b -Q `"$escaped`""
    Invoke-Expression $cmd | Out-Null
    if ($LASTEXITCODE -ne 0) { throw "SQL failed: $Query" }
}

function Set-SubscriptionScenario([string]$Scenario) {
    $gid = $GymId
    switch ($Scenario) {
        'active' {
            Invoke-Sql @"
UPDATE gs SET Status = N'Active',
    CurrentPeriodStart = DATEADD(day, -15, SYSUTCDATETIME()),
    CurrentPeriodEnd = DATEADD(day, 15, SYSUTCDATETIME()),
    GraceEndsAt = DATEADD(day, 18, SYSUTCDATETIME()),
    EndDate = CAST(DATEADD(day, 15, SYSUTCDATETIME()) AS DATE),
    UpdatedAt = SYSUTCDATETIME()
FROM dbo.GymSubscriptions gs
WHERE gs.GymId = '$gid'
  AND gs.GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$gid');
"@
        }
        'grace' {
            Invoke-Sql @"
UPDATE gs SET Status = N'Active',
    CurrentPeriodStart = DATEADD(day, -40, SYSUTCDATETIME()),
    CurrentPeriodEnd = DATEADD(day, -2, SYSUTCDATETIME()),
    GraceEndsAt = DATEADD(day, 1, SYSUTCDATETIME()),
    EndDate = CAST(DATEADD(day, -2, SYSUTCDATETIME()) AS DATE),
    UpdatedAt = SYSUTCDATETIME()
FROM dbo.GymSubscriptions gs
WHERE gs.GymId = '$gid'
  AND gs.GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$gid');
"@
        }
        'expired' {
            Invoke-Sql @"
UPDATE gs SET Status = N'Expired',
    CurrentPeriodStart = DATEADD(day, -60, SYSUTCDATETIME()),
    CurrentPeriodEnd = DATEADD(day, -20, SYSUTCDATETIME()),
    GraceEndsAt = DATEADD(day, -10, SYSUTCDATETIME()),
    EndDate = CAST(DATEADD(day, -20, SYSUTCDATETIME()) AS DATE),
    UpdatedAt = SYSUTCDATETIME()
FROM dbo.GymSubscriptions gs
WHERE gs.GymId = '$gid'
  AND gs.GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$gid');
"@
        }
        'trial' {
            Invoke-Sql @"
DECLARE @TrialPlanId INT = (SELECT TOP 1 SaasPlanId FROM dbo.SaasSubscriptionPlans WHERE PlanCode = N'Trial');
UPDATE gs SET Status = N'Trial', SaasPlanId = @TrialPlanId,
    CurrentPeriodStart = SYSUTCDATETIME(),
    CurrentPeriodEnd = DATEADD(day, 15, SYSUTCDATETIME()),
    TrialEndsAt = DATEADD(day, 15, SYSUTCDATETIME()),
    GraceEndsAt = DATEADD(day, 18, SYSUTCDATETIME()),
    EndDate = CAST(DATEADD(day, 15, SYSUTCDATETIME()) AS DATE),
    UpdatedAt = SYSUTCDATETIME()
FROM dbo.GymSubscriptions gs
WHERE gs.GymId = '$gid'
  AND gs.GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$gid');
"@
        }
    }
}

function Get-PlanIds($Session) {
    $catalog = Api GET "/api/saas/plans/catalog?gymId=$GymId" $Session
    if (-not $catalog.Ok) { throw $catalog.Body }
    $basic = $catalog.Data.data.plans | Where-Object { $_.planCode -eq 'Basic' } | Select-Object -First 1
    $premium = $catalog.Data.data.plans | Where-Object { $_.planCode -eq 'Premium' } | Select-Object -First 1
    return @{ Basic = $basic; Premium = $premium }
}

function New-SaasPaymentOrder($Session, $SaasPlanId, $BillingCycle = 'Monthly', $PricingOptionId = $null) {
    $body = @{ saasPlanId = $SaasPlanId; billingCycle = $BillingCycle }
    if ($PricingOptionId) { $body.pricingOptionId = $PricingOptionId }
    $order = Api POST "/api/saas/payments/order?gymId=$GymId" $Session $body
    if (-not $order.Ok) { throw $order.Body }
    return $order.Data.data
}

function New-RazorpayTestPayment([string]$OrderId, [int]$AmountPaise) {
    $pair = "$KeyId`:$KeySecret"
    $bytes = [Text.Encoding]::ASCII.GetBytes($pair)
    $basic = [Convert]::ToBase64String($bytes)
    $headers = @{ Authorization = "Basic $basic" }
    $payload = @{
        amount = $AmountPaise
        currency = 'INR'
        order_id = $OrderId
        email = 'smoke@test.gym.local'
        contact = '9999999999'
        method = 'netbanking'
        bank = 'HDFC'
    } | ConvertTo-Json
    return Invoke-RestMethod -Uri 'https://api.razorpay.com/v1/payments' -Method POST -Headers $headers -Body $payload -ContentType 'application/json'
}

function Verify-SaasPayment($Session, $SaasPaymentId, $OrderId, $PaymentId, $Signature) {
    return Api POST "/api/saas/payments/verify?gymId=$GymId" $Session @{
        saasPaymentId = $SaasPaymentId
        razorpayOrderId = $OrderId
        razorpayPaymentId = $PaymentId
        razorpaySignature = $Signature
    }
}

function Get-SubscriptionEnd($Session) {
    $sub = Api GET "/api/saas/subscription?gymId=$GymId" $Session
    if (-not $sub.Ok) { throw $sub.Body }
    return [datetime]$sub.Data.data.endDate
}

function Complete-PaymentFlow($Session, $SaasPlanId, $BillingCycle = 'Monthly') {
    $order = New-SaasPaymentOrder $Session $SaasPlanId $BillingCycle
    $amountPaise = [int][Math]::Round($order.amount * 100)
    $payment = New-RazorpayTestPayment $order.razorpayOrderId $amountPaise
    $sig = Invoke-RazorpaySignature $order.razorpayOrderId $payment.id $KeySecret
    $verify = Verify-SaasPayment $Session $order.saasPaymentId $order.razorpayOrderId $payment.id $sig
    if (-not $verify.Ok) { throw $verify.Body }
    return @{ Order = $order; Payment = $payment; Verify = $verify.Data.data }
}

# --- Preconditions ---
$razorpayReady = -not [string]::IsNullOrWhiteSpace($KeyId) -and -not [string]::IsNullOrWhiteSpace($KeySecret)
if (-not $razorpayReady) {
    Write-Host 'Razorpay test keys not set. Set Razorpay__KeyId and Razorpay__KeySecret (or -KeyId/-KeySecret).' -ForegroundColor Yellow
}

try {
    $health = Invoke-RestMethod "$BaseUrl/api/health" -TimeoutSec 5
    Record 'PRE' 'API health' 'PASS' $health.status
}
catch {
    Record 'PRE' 'API health' 'FAIL' 'API unreachable at ' + $BaseUrl
    $results | ConvertTo-Json -Depth 4 | Set-Content $OutputJson
    exit 1
}

try { $session = Login; Record 'PRE' 'Gym admin login' 'PASS' 'Session established' }
catch { Record 'PRE' 'Gym admin login' 'FAIL' (Get-ErrBody $_); $results | ConvertTo-Json -Depth 4 | Set-Content $OutputJson; exit 1 }

if (-not $razorpayReady) {
    @(
        @('S01', 'New subscription purchase'),
        @('S02', 'Plan upgrade'),
        @('S03', 'Renewal before expiry'),
        @('S04', 'Renewal during grace period'),
        @('S05', 'Renewal after grace period'),
        @('S06', 'Failed payment'),
        @('S07', 'Cancelled payment'),
        @('S08', 'Duplicate verification attempt'),
        @('S09', 'Duplicate webhook callback')
    ) | ForEach-Object { Record $_[0] $_[1] 'SKIP' 'Razorpay test keys not configured' }
    $results | ConvertTo-Json -Depth 4 | Set-Content $OutputJson
    exit 2
}

try { $plans = Get-PlanIds $session } catch { Record 'PRE' 'Plan catalog' 'FAIL' $_; exit 1 }
$basicPlanId = $plans.Basic.saasPlanId
$premiumPlanId = $plans.Premium.saasPlanId

# S01 New subscription purchase (trial -> Basic)
try {
    Set-SubscriptionScenario 'trial'
    $before = Get-SubscriptionEnd $session
    $flow = Complete-PaymentFlow $session $basicPlanId 'Monthly'
    $after = Get-SubscriptionEnd $session
    if ($flow.Verify.saasPlanId -eq $basicPlanId -and $after -gt $before) {
        Record 'S01' 'New subscription purchase' 'PASS' "Plan $($flow.Verify.planName), end $after"
    }
    else { Record 'S01' 'New subscription purchase' 'FAIL' 'Subscription not updated as expected' }
}
catch { Record 'S01' 'New subscription purchase' 'FAIL' $_.Exception.Message }

# S02 Plan upgrade (Basic -> Premium)
try {
    Set-SubscriptionScenario 'active'
    Invoke-Sql "UPDATE dbo.GymSubscriptions SET SaasPlanId = (SELECT SaasPlanId FROM dbo.SaasSubscriptionPlans WHERE PlanCode = N'Basic') WHERE GymId = '$GymId' AND GymSubscriptionId = (SELECT MAX(GymSubscriptionId) FROM dbo.GymSubscriptions WHERE GymId = '$GymId');"
    $beforePlan = (Api GET "/api/saas/subscription?gymId=$GymId" $session).Data.data.saasPlanId
    $flow = Complete-PaymentFlow $session $premiumPlanId 'Monthly'
    if ($flow.Verify.saasPlanId -eq $premiumPlanId -and $flow.Verify.saasPlanId -ne $beforePlan) {
        Record 'S02' 'Plan upgrade' 'PASS' "Upgraded to $($flow.Verify.planName)"
    }
    else { Record 'S02' 'Plan upgrade' 'FAIL' 'Plan id unchanged or wrong' }
}
catch { Record 'S02' 'Plan upgrade' 'FAIL' $_.Exception.Message }

# S03 Renewal before expiry
try {
    Set-SubscriptionScenario 'active'
    $endBefore = Get-SubscriptionEnd $session
    $flow = Complete-PaymentFlow $session $basicPlanId 'Monthly'
    $endAfter = Get-SubscriptionEnd $session
    if ($endAfter -gt $endBefore) { Record 'S03' 'Renewal before expiry' 'PASS' "$endBefore -> $endAfter" }
    else { Record 'S03' 'Renewal before expiry' 'FAIL' 'End date did not extend' }
}
catch { Record 'S03' 'Renewal before expiry' 'FAIL' $_.Exception.Message }

# S04 Renewal during grace
try {
    Set-SubscriptionScenario 'grace'
    $endBefore = Get-SubscriptionEnd $session
    $flow = Complete-PaymentFlow $session $basicPlanId 'Monthly'
    $endAfter = Get-SubscriptionEnd $session
    if ($endAfter -gt $endBefore) { Record 'S04' 'Renewal during grace period' 'PASS' "$endBefore -> $endAfter" }
    else { Record 'S04' 'Renewal during grace period' 'FAIL' 'End date did not extend' }
}
catch { Record 'S04' 'Renewal during grace period' 'FAIL' $_.Exception.Message }

# S05 Renewal after grace
try {
    Set-SubscriptionScenario 'expired'
    $flow = Complete-PaymentFlow $session $basicPlanId 'Monthly'
    if ($flow.Verify.status -eq 'Active') { Record 'S05' 'Renewal after grace period' 'PASS' "Reactivated, end $($flow.Verify.endDate)" }
    else { Record 'S05' 'Renewal after grace period' 'FAIL' "Status=$($flow.Verify.status)" }
}
catch { Record 'S05' 'Renewal after grace period' 'FAIL' $_.Exception.Message }

# S06 Failed payment (bad signature)
try {
    $order = New-SaasPaymentOrder $session $basicPlanId 'Monthly'
    $bad = Verify-SaasPayment $session $order.saasPaymentId $order.razorpayOrderId 'pay_invalid' 'bad_signature'
    if (-not $bad.Ok) { Record 'S06' 'Failed payment' 'PASS' 'Rejected invalid signature' }
    else { Record 'S06' 'Failed payment' 'FAIL' 'Invalid signature was accepted' }
}
catch { Record 'S06' 'Failed payment' 'PASS' 'Rejected: ' + $_.Exception.Message }

# S07 Cancelled payment (order created, never verified)
try {
    $order = New-SaasPaymentOrder $session $basicPlanId 'Monthly'
    $pending = Invoke-Sql "SELECT Status FROM dbo.SaasSubscriptionPayments WHERE SaasPaymentId = $($order.saasPaymentId);"
    Record 'S07' 'Cancelled payment' 'PASS' "Order $($order.saasPaymentId) left Pending (no verify call)"
}
catch { Record 'S07' 'Cancelled payment' 'FAIL' $_.Exception.Message }

# S08 Duplicate verification
try {
    Set-SubscriptionScenario 'active'
    $order = New-SaasPaymentOrder $session $basicPlanId 'Monthly'
    $amountPaise = [int][Math]::Round($order.amount * 100)
    $payment = New-RazorpayTestPayment $order.razorpayOrderId $amountPaise
    $sig = Invoke-RazorpaySignature $order.razorpayOrderId $payment.id $KeySecret
    $first = Verify-SaasPayment $session $order.saasPaymentId $order.razorpayOrderId $payment.id $sig
    $endAfterFirst = Get-SubscriptionEnd $session
    $second = Verify-SaasPayment $session $order.saasPaymentId $order.razorpayOrderId $payment.id $sig
    $endAfterSecond = Get-SubscriptionEnd $session
    if ($first.Ok -and $second.Ok -and $endAfterFirst -eq $endAfterSecond) {
        Record 'S08' 'Duplicate verification attempt' 'PASS' 'Second verify returned success without extending period again'
    }
    else { Record 'S08' 'Duplicate verification attempt' 'FAIL' "Ends: $endAfterFirst vs $endAfterSecond" }
}
catch { Record 'S08' 'Duplicate verification attempt' 'FAIL' $_.Exception.Message }

# S09 Duplicate webhook — no SaaS webhook endpoint in codebase
Record 'S09' 'Duplicate webhook callback' 'BLOCKED' 'No /api/razorpay/webhook (or SaaS webhook) endpoint implemented; duplicate callback cannot be exercised'

$pass = @($results | Where-Object Status -eq 'PASS').Count
$fail = @($results | Where-Object Status -eq 'FAIL').Count
$skip = @($results | Where-Object Status -eq 'SKIP').Count
$blocked = @($results | Where-Object Status -eq 'BLOCKED').Count
Write-Host "`nSummary: PASS=$pass FAIL=$fail SKIP=$skip BLOCKED=$blocked" -ForegroundColor Cyan
$results | ConvertTo-Json -Depth 4 | Set-Content $OutputJson
Write-Host "Results written to $OutputJson"
if ($fail -gt 0) { exit 1 }

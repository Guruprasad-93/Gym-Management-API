# Final Pre-Demo QA — role-based API validation (maps to Angular routes)
param(
    [string]$BaseUrl = 'http://localhost:5088',
    [string]$OutputJson = 'pre-demo-qa-results.json'
)

$ErrorActionPreference = 'Continue'
$results = @()
$issues = @()

function Record($Role, $Page, $Api, $Pass, $Status, $Detail) {
    $script:results += [PSCustomObject]@{
        Role = $Role; Page = $Page; Api = $Api
        Result = $(if ($Pass) { 'PASS' } else { 'FAIL' })
        Status = $Status; Detail = $Detail
    }
    if (-not $Pass) {
        $script:issues += "$Role | $Page | $Api | $Detail"
    }
}

function Login($loginId, $password) {
    try {
        $csrf = Invoke-RestMethod -Uri "$BaseUrl/api/auth/csrf" -SessionVariable sess
        $r = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST `
            -Body (@{ loginIdentifier = $loginId; password = $password } | ConvertTo-Json) `
            -ContentType 'application/json' -WebSession $sess
        return @{ Ok = $true; Session = $sess }
    } catch {
        return @{ Ok = $false; Status = [int]$_.Exception.Response.StatusCode; Detail = $_.Exception.Message }
    }
}

function Api($method, $path, $session, $body = $null) {
    try {
        $p = @{ Uri = "$BaseUrl$path"; Method = $method; WebSession = $session; UseBasicParsing = $true }
        if ($session.Cookies.GetCookies($BaseUrl)['XSRF-TOKEN']) {
            $p.Headers = @{ 'X-XSRF-TOKEN' = $session.Cookies.GetCookies($BaseUrl)['XSRF-TOKEN'].Value }
        }
        if ($body) { $p.ContentType = 'application/json'; $p.Body = ($body | ConvertTo-Json -Depth 8) }
        $r = Invoke-WebRequest @p
        return @{ Ok = $true; Status = [int]$r.StatusCode; Body = $r.Content }
    } catch {
        $code = 0; try { $code = [int]$_.Exception.Response.StatusCode } catch {}
        return @{ Ok = $false; Status = $code; Body = $_.Exception.Message }
    }
}

$roles = @(
    @{
        Name = 'SuperAdmin'; Login = 'superadmin'; Password = 'SuperAdmin@123'
        Pages = @(
            @{ Page = 'Platform Gyms'; Api = 'GET /api/gyms' }
            @{ Page = 'SaaS Platform Dashboard'; Api = 'GET /api/saas/platform/dashboard' }
            @{ Page = 'Subscription Plans'; Api = 'GET /api/platform/subscription-plans' }
            @{ Page = 'Tenant Menus'; Api = 'GET /api/platform/tenant-menus/gyms' }
        )
    }
    @{
        Name = 'GymAdmin'; Login = 'fitzone_admin'; Password = 'Demo@123'
        Pages = @(
            @{ Page = 'Dashboard'; Api = 'GET /api/dashboard/stats' }
            @{ Page = 'Members'; Api = 'GET /api/members?pageNumber=1&pageSize=10' }
            @{ Page = 'Trainers'; Api = 'GET /api/trainers?pageNumber=1&pageSize=10' }
            @{ Page = 'Membership Plans'; Api = 'GET /api/membership-plans' }
            @{ Page = 'Memberships'; Api = 'GET /api/memberships?pageNumber=1&pageSize=10' }
            @{ Page = 'Payments'; Api = 'GET /api/payments?pageNumber=1&pageSize=10' }
            @{ Page = 'Attendance'; Api = 'GET /api/attendance/dashboard' }
            @{ Page = 'Leads'; Api = 'GET /api/leads?pageNumber=1&pageSize=10' }
            @{ Page = 'Lead Dashboard'; Api = 'GET /api/leads/dashboard' }
            @{ Page = 'Branches'; Api = 'GET /api/branches/list' }
            @{ Page = 'Branch Dashboard'; Api = 'GET /api/branches/dashboard' }
            @{ Page = 'Expenses'; Api = 'GET /api/expenses?pageNumber=1&pageSize=10' }
            @{ Page = 'Payroll'; Api = 'GET /api/payroll?pageNumber=1&pageSize=10' }
            @{ Page = 'Financial'; Api = 'GET /api/financial/dashboard' }
            @{ Page = 'Analytics'; Api = 'GET /api/analytics/dashboard' }
            @{ Page = 'Workout Plans'; Api = 'GET /api/workout-plans' }
            @{ Page = 'Diet Plans'; Api = 'GET /api/diet-plans' }
            @{ Page = 'Notifications'; Api = 'GET /api/notifications/dashboard' }
            @{ Page = 'Bookings'; Api = 'GET /api/bookings?pageNumber=1&pageSize=10' }
            @{ Page = 'Schedules'; Api = 'GET /api/schedules' }
            @{ Page = 'AI Insights'; Api = 'GET /api/ai/dashboard' }
            @{ Page = 'Website Builder'; Api = 'GET /api/website/settings' }
            @{ Page = 'White Label'; Api = 'GET /api/white-label/settings' }
            @{ Page = 'Subscription'; Api = 'GET /api/saas/subscription' }
            @{ Page = 'Audit Logs'; Api = 'GET /api/audit-logs?pageNumber=1&pageSize=10' }
            @{ Page = 'Announcements'; Api = 'GET /api/branches/announcements' }
        )
    }
    @{
        Name = 'Trainer'; Login = 'fitzone_trainer1'; Password = 'Demo@123'
        Pages = @(
            @{ Page = 'Dashboard'; Api = 'GET /api/dashboard/stats' }
            @{ Page = 'Members'; Api = 'GET /api/members?pageNumber=1&pageSize=10' }
            @{ Page = 'Attendance'; Api = 'GET /api/attendance/today' }
            @{ Page = 'Workout Plans'; Api = 'GET /api/workout-plans' }
            @{ Page = 'Diet Plans'; Api = 'GET /api/diet-plans' }
            @{ Page = 'Leads'; Api = 'GET /api/leads?pageNumber=1&pageSize=10' }
            @{ Page = 'AI Recommendations'; Api = 'GET /api/ai/recommendations' }
            @{ Page = 'Bookings'; Api = 'GET /api/bookings?pageNumber=1&pageSize=10' }
        )
    }
    @{
        Name = 'Member'; Login = 'fitzone_member001'; Password = 'Demo@123'
        Pages = @(
            @{ Page = 'Dashboard'; Api = 'GET /api/member/dashboard' }
            @{ Page = 'Workout Plan'; Api = 'GET /api/workout-plans/members/me' }
            @{ Page = 'Diet Plan'; Api = 'GET /api/diet-plans/members/me' }
            @{ Page = 'Session Profile'; Api = 'GET /api/auth/session' }
            @{ Page = 'Notifications'; Api = 'GET /api/mobile/notifications' }
            @{ Page = 'Razorpay Context'; Api = 'GET /api/payments/razorpay/checkout-context' }
        )
    }
)

Write-Host "Pre-Demo QA against $BaseUrl"

foreach ($role in $roles) {
    $auth = Login $role.Login $role.Password
    if (-not $auth.Ok) {
        Record $role.Name 'Login' 'POST /api/auth/login' $false $auth.Status $auth.Detail
        continue
    }
    Record $role.Name 'Login' 'POST /api/auth/login' $true 200 'OK'
    foreach ($p in $role.Pages) {
        $parts = $p.Api -split ' ', 2
        $r = Api $parts[0] $parts[1] $auth.Session
        Record $role.Name $p.Page $p.Api $r.Ok $r.Status $(if ($r.Ok) { 'OK' } else { $r.Body })
    }
}

# Razorpay mock E2E — use a member unlikely to have a stale pending order from prior QA runs
$memberAuth = Login 'fitzone_member050' 'Demo@123'
if ($memberAuth.Ok) {
    $ctx = Api GET '/api/payments/razorpay/checkout-context' $memberAuth.Session
    if ($ctx.Ok) {
        $ctxJson = $ctx.Body | ConvertFrom-Json
        $membershipId = $ctxJson.data.membershipId
        $order = Api POST '/api/payments/razorpay/order' $memberAuth.Session @{ membershipId = $membershipId }
        if ($order.Ok) {
            $orderJson = $order.Body | ConvertFrom-Json
            $orderId = $orderJson.data.razorpayOrderId
            $paymentId = "pay_mock_$([guid]::NewGuid().ToString('N'))"
            $hmac = New-Object System.Security.Cryptography.HMACSHA256
            $hmac.Key = [Text.Encoding]::UTF8.GetBytes('mock_razorpay_dev_secret')
            $hash = $hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes("$orderId|$paymentId"))
            $sig = ([BitConverter]::ToString($hash) -replace '-', '').ToLower()
            $verify = Api POST '/api/payments/razorpay/verify' $memberAuth.Session @{
                razorpayOrderId = $orderId; razorpayPaymentId = $paymentId; razorpaySignature = $sig
            }
            Record 'Member' 'Razorpay Payment' 'POST /api/payments/razorpay/verify' $verify.Ok $verify.Status $(if ($verify.Ok) { 'Payment verified' } else { $verify.Body })
        } else {
            Record 'Member' 'Razorpay Order' 'POST /api/payments/razorpay/order' $false $order.Status $order.Body
        }
    } else {
        Record 'Member' 'Razorpay Context' 'GET /api/payments/razorpay/checkout-context' $false $ctx.Status $ctx.Body
    }
}

$passed = @($results | Where-Object Result -eq 'PASS').Count
$failed = @($results | Where-Object Result -eq 'FAIL').Count
$summary = [ordered]@{
    Timestamp = (Get-Date).ToString('o')
    BaseUrl = $BaseUrl
    PagesTested = $results.Count
    Passed = $passed
    Failed = $failed
    IssuesFound = $issues.Count
    Tests = $results
    Issues = $issues
}
$outPath = Join-Path $PSScriptRoot $OutputJson
$summary | ConvertTo-Json -Depth 6 | Set-Content $outPath -Encoding UTF8

Write-Host "`n=== Pre-Demo QA: $passed passed, $failed failed ==="
Write-Host "Report: $outPath"
if ($failed -gt 0) { exit 1 }

# UI Page QA — validates API calls each Angular page makes on load
param(
    [string]$BaseUrl = 'http://localhost:5088',
    [string]$OutputJson = 'ui-page-qa-results.json'
)

$ErrorActionPreference = 'Continue'
$results = @()

function Record($Role, $Page, $Api, $Pass, $Status, $Detail) {
    $script:results += [PSCustomObject]@{
        Role = $Role; Page = $Page; Api = $Api
        Result = $(if ($Pass) { 'PASS' } else { 'FAIL' })
        Status = $Status; Detail = $Detail
    }
}

function Login($loginId, $password) {
    try {
        $null = Invoke-RestMethod -Uri "$BaseUrl/api/auth/csrf" -SessionVariable sess
        $null = Invoke-RestMethod -Uri "$BaseUrl/api/auth/login" -Method POST `
            -Body (@{ loginIdentifier = $loginId; password = $password } | ConvertTo-Json) `
            -ContentType 'application/json' -WebSession $sess
        return @{ Ok = $true; Session = $sess }
    } catch {
        return @{ Ok = $false; Status = [int]$_.Exception.Response.StatusCode; Detail = $_.Exception.Message }
    }
}

function Api($method, $path, $session) {
    try {
        $p = @{ Uri = "$BaseUrl$path"; Method = $method; WebSession = $session; UseBasicParsing = $true }
        if ($session.Cookies.GetCookies($BaseUrl)['XSRF-TOKEN']) {
            $p.Headers = @{ 'X-XSRF-TOKEN' = $session.Cookies.GetCookies($BaseUrl)['XSRF-TOKEN'].Value }
        }
        $r = Invoke-WebRequest @p
        return @{ Ok = $true; Status = [int]$r.StatusCode; Body = $r.Content }
    } catch {
        $code = 0; try { $code = [int]$_.Exception.Response.StatusCode } catch {}
        $body = $_.Exception.Message
        try {
            $reader = [IO.StreamReader]::new($_.Exception.Response.GetResponseStream())
            $body = $reader.ReadToEnd()
        } catch {}
        return @{ Ok = $false; Status = $code; Body = $body }
    }
}

$roles = @(
    @{
        Name = 'SuperAdmin'; Login = 'superadmin'; Password = 'SuperAdmin@123'
        Pages = @(
            @{ Page = 'Dashboard'; Api = 'GET /api/saas/platform/dashboard' }
            @{ Page = 'Gyms'; Api = 'GET /api/gyms' }
            @{ Page = 'Gym Admins'; Api = 'GET /api/gym-admins?pageNumber=1&pageSize=10' }
            @{ Page = 'Subscription Plans'; Api = 'GET /api/platform/subscription-plans' }
            @{ Page = 'Roles'; Api = 'GET /api/roles' }
            @{ Page = 'Privileges'; Api = 'GET /api/privileges' }
            @{ Page = 'Role Matrix'; Api = 'GET /api/role-privileges/matrix' }
            @{ Page = 'Audit Logs'; Api = 'GET /api/audit-logs?pageNumber=1&pageSize=10' }
            @{ Page = 'White Label Platform'; Api = 'GET /api/platform/white-label/dashboard' }
        )
    }
    @{
        Name = 'GymAdmin'; Login = 'fitzone_admin'; Password = 'Demo@123'
        Pages = @(
            @{ Page = 'Dashboard'; Api = 'GET /api/dashboard/stats' }
            @{ Page = 'Analytics Revenue'; Api = 'GET /api/analytics/revenue' }
            @{ Page = 'Analytics Members'; Api = 'GET /api/analytics/members' }
            @{ Page = 'Analytics Attendance'; Api = 'GET /api/analytics/attendance' }
            @{ Page = 'Analytics Trainers'; Api = 'GET /api/analytics/trainers' }
            @{ Page = 'Members'; Api = 'GET /api/members?pageNumber=1&pageSize=10' }
            @{ Page = 'Leads'; Api = 'GET /api/leads?pageNumber=1&pageSize=10' }
            @{ Page = 'Lead Followups'; Api = 'GET /api/leads/followups/pending' }
            @{ Page = 'Lead Trials'; Api = 'GET /api/leads/trials/today' }
            @{ Page = 'Lead Analytics'; Api = 'GET /api/leads/dashboard' }
            @{ Page = 'Expenses'; Api = 'GET /api/expenses?pageNumber=1&pageSize=10' }
            @{ Page = 'Payroll'; Api = 'GET /api/payroll?pageNumber=1&pageSize=10' }
            @{ Page = 'Financial'; Api = 'GET /api/financial/dashboard' }
            @{ Page = 'Trainers'; Api = 'GET /api/trainers?pageNumber=1&pageSize=10' }
            @{ Page = 'Membership Plans'; Api = 'GET /api/membership-plans' }
            @{ Page = 'Memberships'; Api = 'GET /api/memberships?pageNumber=1&pageSize=10' }
            @{ Page = 'Payments'; Api = 'GET /api/payments?pageNumber=1&pageSize=10' }
            @{ Page = 'Revenue'; Api = 'GET /api/payments/revenue/dashboard' }
            @{ Page = 'Attendance'; Api = 'GET /api/attendance/dashboard' }
            @{ Page = 'Attendance List'; Api = 'GET /api/attendance?pageNumber=1&pageSize=10' }
            @{ Page = 'Audit Logs'; Api = 'GET /api/audit-logs?pageNumber=1&pageSize=10' }
            @{ Page = 'Notifications'; Api = 'GET /api/notifications/dashboard' }
            @{ Page = 'Notification Templates'; Api = 'GET /api/notifications/templates' }
            @{ Page = 'Diet Plans'; Api = 'GET /api/diet-plans' }
            @{ Page = 'Workout Plans'; Api = 'GET /api/workout-plans' }
            @{ Page = 'Subscription'; Api = 'GET /api/saas/subscription' }
            @{ Page = 'Gym Branding'; Api = 'GET /api/saas/branding' }
            @{ Page = 'Gym Branding Logo'; Api = 'GET /api/files/gym/logo' }
            @{ Page = 'White Label'; Api = 'GET /api/white-label/settings' }
            @{ Page = 'White Label Mobile'; Api = 'GET /api/white-label/mobile-settings' }
            @{ Page = 'White Label Preview'; Api = 'GET /api/white-label/preview' }
            @{ Page = 'Branches'; Api = 'GET /api/branches/list' }
            @{ Page = 'Branch Dashboard'; Api = 'GET /api/branches/dashboard' }
            @{ Page = 'Branch Analytics'; Api = 'GET /api/branches/analytics' }
            @{ Page = 'Mobile Notifications'; Api = 'GET /api/members?pageNumber=1&pageSize=20' }
            @{ Page = 'AI Dashboard'; Api = 'GET /api/ai/dashboard' }
            @{ Page = 'AI Insights'; Api = 'GET /api/ai/business-insights?pageNumber=1&pageSize=20' }
            @{ Page = 'Bookings'; Api = 'GET /api/bookings?pageNumber=1&pageSize=10' }
            @{ Page = 'Schedules'; Api = 'GET /api/schedules' }
            @{ Page = 'Booking Analytics'; Api = 'GET /api/booking-analytics?days=30' }
            @{ Page = 'Website Builder'; Api = 'GET /api/website/settings' }
            @{ Page = 'Website Pages'; Api = 'GET /api/website/pages' }
            @{ Page = 'Website Gallery'; Api = 'GET /api/website/gallery' }
            @{ Page = 'Website Testimonials'; Api = 'GET /api/website/testimonials' }
            @{ Page = 'Website Analytics'; Api = 'GET /api/website/analytics' }
        )
    }
    @{
        Name = 'Trainer'; Login = 'fitzone_trainer1'; Password = 'Demo@123'
        Pages = @(
            @{ Page = 'Dashboard'; Api = 'GET /api/dashboard/stats' }
            @{ Page = 'Members'; Api = 'GET /api/members?pageNumber=1&pageSize=10' }
            @{ Page = 'Leads'; Api = 'GET /api/leads?pageNumber=1&pageSize=10' }
            @{ Page = 'Attendance'; Api = 'GET /api/attendance/today' }
            @{ Page = 'Workout Plans'; Api = 'GET /api/workout-plans' }
            @{ Page = 'AI Recommendations'; Api = 'GET /api/ai/recommendations' }
            @{ Page = 'Schedule'; Api = 'GET /api/schedules' }
            @{ Page = 'Bookings'; Api = 'GET /api/bookings?pageNumber=1&pageSize=10' }
        )
    }
    @{
        Name = 'Member'; Login = 'fitzone_member001'; Password = 'Demo@123'
        Pages = @(
            @{ Page = 'Dashboard'; Api = 'GET /api/member/dashboard' }
            @{ Page = 'Profile'; Api = 'GET /api/auth/session' }
            @{ Page = 'Goals'; Api = 'GET /api/member/goals' }
            @{ Page = 'Progress'; Api = 'GET /api/member/progress' }
            @{ Page = 'Workouts'; Api = 'GET /api/member/workouts' }
            @{ Page = 'Diet Tracker'; Api = 'GET /api/member/diets' }
            @{ Page = 'Water'; Api = 'GET /api/member/water' }
            @{ Page = 'Referrals'; Api = 'GET /api/member/referrals' }
            @{ Page = 'Diet Plan'; Api = 'GET /api/diet-plans/members/me' }
            @{ Page = 'Workout Plan'; Api = 'GET /api/workout-plans/members/me' }
            @{ Page = 'Checkout'; Api = 'GET /api/payments/razorpay/checkout-context' }
            @{ Page = 'Bookings'; Api = 'GET /api/bookings?pageNumber=1&pageSize=10' }
            @{ Page = 'Booking History'; Api = 'GET /api/bookings?pageNumber=1&pageSize=50' }
        )
    }
)

Write-Host "UI Page QA against $BaseUrl"

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

$passed = @($results | Where-Object Result -eq 'PASS').Count
$failed = @($results | Where-Object Result -eq 'FAIL').Count
$summary = [ordered]@{
    Timestamp = (Get-Date).ToString('o')
    BaseUrl = $BaseUrl
    PagesTested = $results.Count
    Passed = $passed
    Failed = $failed
    Tests = $results
    Issues = @($results | Where-Object Result -eq 'FAIL' | ForEach-Object { "$($_.Role) | $($_.Page) | $($_.Api) | $($_.Detail)" })
}
$outPath = Join-Path $PSScriptRoot $OutputJson
$summary | ConvertTo-Json -Depth 6 | Set-Content $outPath -Encoding UTF8
Write-Host "`n=== UI Page QA: $passed passed, $failed failed ==="
Write-Host "Report: $outPath"
if ($failed -gt 0) { exit 1 }

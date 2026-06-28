# MVP QA validation - API smoke tests for demo environment
$ErrorActionPreference = 'Continue'
$base = if ($env:API_BASE_URL) { $env:API_BASE_URL } else { "http://localhost:5088" }
$results = @()

function Record-Test($Id, $Module, $Name, $Pass, $Status, $Detail) {
    $script:results += [PSCustomObject]@{
        Id = $Id; Module = $Module; Name = $Name
        Result = $(if ($Pass) { 'PASS' } else { 'FAIL' })
        Status = $Status; Detail = $Detail
    }
}

function Get-ErrBody($err) {
    try { return $err.ErrorDetails.Message } catch { return $err.Exception.Message }
}

function Login($loginId, $password) {
    $body = @{ loginIdentifier = $loginId; password = $password }
    try {
        $r = Invoke-RestMethod -Uri "$base/api/auth/login" -Method POST -Body ($body | ConvertTo-Json) -ContentType "application/json" -SessionVariable sess
        return @{ Ok = $true; Data = $r; Session = $sess }
    } catch {
        return @{ Ok = $false; Status = [int]$_.Exception.Response.StatusCode; Detail = (Get-ErrBody $_) }
    }
}

function Api($method, $path, $session, $body = $null) {
    try {
        $params = @{ Uri = "$base$path"; Method = $method; WebSession = $session; UseBasicParsing = $true; ContentType = 'application/json' }
        if ($body) { $params.Body = ($body | ConvertTo-Json -Depth 8) }
        $r = Invoke-WebRequest @params
        return @{ Ok = $true; Status = [int]$r.StatusCode; Body = $r.Content }
    } catch {
        return @{ Ok = $false; Status = [int]$_.Exception.Response.StatusCode; Body = (Get-ErrBody $_) }
    }
}

Write-Host "MVP QA validation against $base"

$h = try { Invoke-RestMethod "$base/api/health" -TimeoutSec 120 } catch { $null }
Record-Test "H1" "Health" "API health" ($null -ne $h) $(if ($h) { 200 } else { 0 }) $(if ($h) { 'Healthy' } else { 'Unreachable' })

$super = Login "superadmin" "SuperAdmin@123"
Record-Test "A1" "Auth" "Super Admin login" $super.Ok $(if ($super.Ok) { 200 } else { $super.Status }) $(if ($super.Ok) { 'OK' } else { $super.Detail })

$admin = Login "fitzone_admin" "Demo@123"
Record-Test "A2" "Auth" "Gym Admin login" $admin.Ok $(if ($admin.Ok) { 200 } else { $admin.Status }) $(if ($admin.Ok) { 'OK' } else { $admin.Detail })

$trainer = Login "fitzone_trainer1" "Demo@123"
Record-Test "A3" "Auth" "Trainer login" $trainer.Ok $(if ($trainer.Ok) { 200 } else { $trainer.Status }) $(if ($trainer.Ok) { 'OK' } else { $trainer.Detail })

$member = Login "fitzone_member001" "Demo@123"
Record-Test "A4" "Auth" "Member login" $member.Ok $(if ($member.Ok) { 200 } else { $member.Status }) $(if ($member.Ok) { 'OK' } else { $member.Detail })

if ($admin.Ok) {
    $s = $admin.Session
    $modules = @(
        @{ Id = "M1"; Name = "Members list"; Path = "/api/members?pageNumber=1&pageSize=10" }
        @{ Id = "M2"; Name = "Trainers list"; Path = "/api/trainers?pageNumber=1&pageSize=10" }
        @{ Id = "M3"; Name = "Membership plans"; Path = "/api/membership-plans" }
        @{ Id = "M4"; Name = "Memberships"; Path = "/api/memberships?pageNumber=1&pageSize=10" }
        @{ Id = "M5"; Name = "Payments"; Path = "/api/payments?pageNumber=1&pageSize=10" }
        @{ Id = "M6"; Name = "Attendance dashboard"; Path = "/api/attendance/dashboard" }
        @{ Id = "M7"; Name = "Leads"; Path = "/api/leads?pageNumber=1&pageSize=10" }
        @{ Id = "M8"; Name = "Lead dashboard"; Path = "/api/leads/dashboard" }
        @{ Id = "M9"; Name = "Branches"; Path = "/api/branches" }
        @{ Id = "M10"; Name = "Branch dashboard"; Path = "/api/branches/dashboard" }
        @{ Id = "M11"; Name = "Expenses"; Path = "/api/expenses?pageNumber=1&pageSize=10" }
        @{ Id = "M12"; Name = "Workout plans"; Path = "/api/workout-plans" }
        @{ Id = "M13"; Name = "Diet plans"; Path = "/api/diet-plans" }
        @{ Id = "M14"; Name = "Notifications dashboard"; Path = "/api/notifications/dashboard" }
        @{ Id = "M15"; Name = "Analytics dashboard"; Path = "/api/analytics/dashboard" }
        @{ Id = "M16"; Name = "Audit logs"; Path = "/api/audit-logs?pageNumber=1&pageSize=10" }
        @{ Id = "M17"; Name = "Gym subscription"; Path = "/api/saas/subscription" }
        @{ Id = "M18"; Name = "Announcements"; Path = "/api/branches/announcements" }
    )
    foreach ($m in $modules) {
        $r = Api GET $m.Path $s
        Record-Test $m.Id "Modules" $m.Name $r.Ok $r.Status $(if ($r.Ok) { 'OK' } elseif ($r.Body) { $r.Body.Substring(0, [Math]::Min(120, $r.Body.Length)) } else { "HTTP $($r.Status)" })
    }

    $mem = Api GET "/api/members?pageNumber=1&pageSize=1" $s
    $hasData = $mem.Ok -and ($mem.Body -match '"totalCount"\s*:\s*[1-9]')
    Record-Test "D1" "Data" "Members seeded (totalCount > 0)" $hasData $mem.Status $(if ($hasData) { 'Has members' } else { 'Empty or failed' })
}

if ($super.Ok) {
    $ss = $super.Session
    $gyms = Api GET "/api/gyms" $ss
    Record-Test "P1" "Platform" "Super Admin gyms list" $gyms.Ok $gyms.Status $(if ($gyms.Ok) { 'OK' } else { $gyms.Body })
}

$passed = @($results | Where-Object { $_.Result -eq 'PASS' }).Count
$failed = @($results | Where-Object { $_.Result -eq 'FAIL' }).Count
$summary = [PSCustomObject]@{
    Timestamp = (Get-Date).ToString("o")
    BaseUrl = $base
    TotalTests = $results.Count
    Passed = $passed
    Failed = $failed
    Tests = $results
}
$out = Join-Path $PSScriptRoot "mvp-qa-results.json"
$summary | ConvertTo-Json -Depth 6 | Set-Content $out -Encoding UTF8

Write-Host ""
Write-Host "=== MVP QA Summary ==="
Write-Host "Total: $($results.Count)  Passed: $passed  Failed: $failed"
Write-Host "Results: $out"
if ($failed -gt 0) {
    $results | Where-Object { $_.Result -eq 'FAIL' } | Format-Table Id, Module, Name, Status, Detail -AutoSize
    exit 1
}
exit 0

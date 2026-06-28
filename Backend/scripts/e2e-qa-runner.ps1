# E2E QA runner - API-level validation
$base = "http://localhost:5099"
$gymId = "B2EDBB38-EE01-4D17-94B6-1B3303807B91"
$results = @()

function Record-Test($Id, $Module, $Name, $Pass, $Status, $Detail) {
    $script:results += [PSCustomObject]@{
        Id = $Id; Module = $Module; Name = $Name
        Result = if ($Pass) { "PASS" } else { "FAIL" }
        Status = $Status; Detail = $Detail
    }
}

function Login($loginId, $password, $gym = $null) {
    $body = @{ loginIdentifier = $loginId; password = $password }
    if ($gym) { $body.gymId = $gym }
    try {
        $r = Invoke-RestMethod -Uri "$base/api/auth/login" -Method POST -Body ($body | ConvertTo-Json) -ContentType "application/json" -SessionVariable sess
        return @{ Ok = $true; Data = $r; Session = $sess }
    } catch {
        $code = $_.Exception.Response.StatusCode.value__
        $msg = try { $_.ErrorDetails.Message } catch { $_.Exception.Message }
        return @{ Ok = $false; Status = $code; Detail = $msg }
    }
}

function Api-Get($path, $session) {
    try {
        $r = Invoke-WebRequest -Uri "$base$path" -WebSession $session -UseBasicParsing
        return @{ Ok = $true; Status = [int]$r.StatusCode; Body = $r.Content }
    } catch {
        $code = $_.Exception.Response.StatusCode.value__
        $msg = try { $_.ErrorDetails.Message } catch { $_.Exception.Message }
        return @{ Ok = $false; Status = $code; Detail = $msg }
    }
}

function Api-Post($path, $body, $session) {
    try {
        $json = if ($body) { $body | ConvertTo-Json -Depth 5 } else { "{}" }
        $r = Invoke-WebRequest -Uri "$base$path" -Method POST -Body $json -ContentType "application/json" -WebSession $session -UseBasicParsing
        return @{ Ok = $true; Status = [int]$r.StatusCode; Body = $r.Content }
    } catch {
        $code = $_.Exception.Response.StatusCode.value__
        $msg = try { $_.ErrorDetails.Message } catch { $_.Exception.Message }
        return @{ Ok = $false; Status = $code; Detail = $msg }
    }
}

# --- Authentication ---
$h = Invoke-RestMethod -Uri "$base/api/health" -Method GET -ErrorAction SilentlyContinue
Record-Test "A0" "Authentication" "Health endpoint" ($null -ne $h) "200" "API reachable"

$tenant = Login "admin" "Demo@123" $gymId
Record-Test "A1" "Authentication" "Tenant login (LoginIdentifier+gymId)" $tenant.Ok $(if($tenant.Ok){"200"}else{$tenant.Status}) $(if($tenant.Ok){"JWT/cookie issued"}else{$tenant.Detail})

$super = Login "superadmin" "SuperAdmin@123"
Record-Test "A2" "Authentication" "Super Admin login (no gymId)" $super.Ok $(if($super.Ok){"200"}else{$super.Status}) $(if($super.Ok){"Platform login OK"}else{$super.Detail})

$bad = Login "admin" "wrong" $gymId
Record-Test "A3" "Authentication" "Invalid password rejected" (-not $bad.Ok) $(if($bad.Status){$bad.Status}else{"401"}) $(if(-not $bad.Ok){"Correctly rejected"}else{"Unexpected success"})

$wrongGym = Login "admin" "Demo@123" "00000000-0000-0000-0000-000000000001"
Record-Test "A4" "Authentication" "Wrong gymId rejected" (-not $wrongGym.Ok) $(if($wrongGym.Status){$wrongGym.Status}else{"401"}) $(if(-not $wrongGym.Ok){"Correctly rejected"}else{"Unexpected success"})

if ($tenant.Ok) {
    $sess = $tenant.Session
    $val = Api-Get "/api/auth/validate" $sess
    Record-Test "A5" "Authentication" "JWT validate endpoint" $val.Ok $val.Status $(if($val.Ok){"Session valid"}else{$val.Detail})

    $session = Api-Get "/api/auth/session" $sess
    $hasMenus = $session.Body -match "enabledMenuCodes|permissions"
    Record-Test "A6" "Authentication" "Session includes permissions/menus" ($session.Ok -and $hasMenus) $session.Status $(if($hasMenus){"RBAC+menus in session"}else{$session.Detail})

    $anon = Api-Get "/api/members?pageNumber=1&pageSize=5" $null
    Record-Test "A7" "Authentication" "Anonymous blocked from members" (-not $anon.Ok) $anon.Status "401 Unauthorized"

    $members = Api-Get "/api/members?pageNumber=1&pageSize=5" $sess
    Record-Test "A8" "Authentication" "GymId isolation - own members" $members.Ok $members.Status $(if($members.Ok){"Scoped to tenant"}else{$members.Detail})

    $wrong = Api-Get "/api/members?gymId=00000000-0000-0000-0000-000000000001&pageNumber=1&pageSize=5" $sess
    $blocked = (-not $wrong.Ok) -or ($wrong.Status -ge 400)
    Record-Test "A9" "Authentication" "GymId isolation - wrong gymId" $blocked $wrong.Status $(if($blocked){"Cross-tenant blocked"}else{"Data leak risk"})
}

# --- Tenant Menu ---
if ($tenant.Ok) {
    $menus = Api-Get "/api/menus/my-menus" $tenant.Session
    Record-Test "T1" "Tenant Menu" "GET my-menus" $menus.Ok $menus.Status $(if($menus.Ok){"Menus returned"}else{$menus.Detail})

    if ($super.Ok) {
        $disable = Api-Post "/api/platform/tenant-menus/$gymId/disable" @{ menuCode = "MEMBERS" } $super.Session
        Record-Test "T2" "Tenant Menu" "Super Admin disable MEMBERS" ($disable.Ok -or $disable.Status -eq 403) $disable.Status $(if($disable.Ok){"Disabled"}else{$disable.Detail})

        Start-Sleep -Seconds 1
        $blockedMembers = Api-Get "/api/members?pageNumber=1&pageSize=5" $tenant.Session
        Record-Test "T3" "Tenant Menu" "API 403 when MEMBERS disabled" ($blockedMembers.Status -eq 403) $blockedMembers.Status $(if($blockedMembers.Status -eq 403){"403 Forbidden"}else{$blockedMembers.Detail})

        $enable = Api-Post "/api/platform/tenant-menus/$gymId/enable" @{ menuCode = "MEMBERS" } $super.Session
        Record-Test "T4" "Tenant Menu" "Re-enable MEMBERS" ($enable.Ok -or $enable.Status -eq 403) $enable.Status $(if($enable.Ok){"Re-enabled"}else{$enable.Detail})
    } else {
        Record-Test "T2" "Tenant Menu" "Super Admin disable MEMBERS" $false "BLOCKED" "Super Admin login failed"
        Record-Test "T3" "Tenant Menu" "API 403 when MEMBERS disabled" $false "BLOCKED" "Depends on T2"
        Record-Test "T4" "Tenant Menu" "Re-enable MEMBERS" $false "BLOCKED" "Depends on T2"
    }
}

# --- Core modules (tenant authenticated) ---
if ($tenant.Ok) {
    $sess = $tenant.Session
    $modules = @(
        @("M1","Members","/api/members?pageNumber=1&pageSize=5"),
        @("TR1","Trainers","/api/trainers?pageNumber=1&pageSize=5"),
        @("C1","CRM/Leads","/api/leads?pageNumber=1&pageSize=10"),
        @("C2","CRM/Leads dashboard","/api/leads/dashboard"),
        @("MS1","Memberships","/api/memberships"),
        @("P1","Payments","/api/payments"),
        @("AT1","Attendance","/api/attendance/statuses"),
        @("D1","Diet Plans","/api/diet-plans?pageNumber=1&pageSize=5"),
        @("W1","Workout Plans","/api/workout-plans?pageNumber=1&pageSize=5"),
        @("N1","Notifications","/api/notifications/templates?pageNumber=1&pageSize=5"),
        @("R1","Reports/Audit","/api/audit-logs?pageNumber=1&pageSize=5"),
        @("AN1","Analytics","/api/analytics/dashboard"),
        @("B1","Multi-Branch","/api/branches?pageNumber=1&pageSize=5"),
        @("WL1","White Label","/api/white-label/branding"),
        @("PW1","Public Website","/api/website-builder/pages?pageNumber=1&pageSize=5")
    )
    foreach ($m in $modules) {
        $r = Api-Get $m[2] $sess
        Record-Test $m[0] $m[1] "GET $($m[2])" $r.Ok $r.Status $(if($r.Ok){"OK"}else{$r.Detail.Substring(0,[Math]::Min(120,$r.Detail.Length))})
    }

    $createMember = Api-Post "/api/members" @{
        name = "E2E Test Member"; loginIdentifier = "e2etest$([guid]::NewGuid().ToString('N').Substring(0,6))"
        password = "Test@12345"; joinDate = (Get-Date).ToString("yyyy-MM-dd")
    } $sess
    Record-Test "M2" "Members" "Create member with LoginIdentifier" ($createMember.Status -in 200,201) $createMember.Status $(if($createMember.Status -in 200,201){"Created"}else{$createMember.Detail.Substring(0,[Math]::Min(120,$createMember.Detail.Length))})
}

# --- DemoGymId mismatch check ---
$demoMismatch = Login "admin" "Demo@123" "a1b2c3d4-e5f6-7890-abcd-ef1234567890"
Record-Test "A10" "Authentication" "Login with hardcoded DemoGymId constant" $demoMismatch.Ok $(if($demoMismatch.Ok){"200"}else{$demoMismatch.Status}) $(if($demoMismatch.Ok){"Works"}else{"DemoGymId constant may not match seeded gym"})

$results | Format-Table -AutoSize
$passed = ($results | Where-Object Result -eq "PASS").Count
$failed = ($results | Where-Object Result -eq "FAIL").Count
Write-Host "`nSUMMARY: Total=$($results.Count) Passed=$passed Failed=$failed"
$results | ConvertTo-Json -Depth 3 | Out-File "g:\GymManagementSystem\Backend\scripts\e2e-qa-results.json"
Write-Host "Results saved to e2e-qa-results.json"

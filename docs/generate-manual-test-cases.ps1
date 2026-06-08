# Generates MANUAL_TEST_CASES.md and MANUAL_TEST_CASES.csv
$outDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rows = [System.Collections.Generic.List[object]]::new()

function Add-TC($Module, $Page, $Route, $Event, $Desc, $Pre, $Steps, $Expected, $Priority = 'Medium') {
    $id = "TC-$Module-$($rows.Count + 1)".PadRight(12)
    $rows.Add([PSCustomObject]@{
        TC_ID = "TC-$Module-{0:D3}" -f ($rows.Count + 1)
        Module = $Module
        Page = $Page
        Route = $Route
        Event = $Event
        Description = $Desc
        Preconditions = $Pre
        Steps = $Steps
        Expected_Result = $Expected
        Priority = $Priority
        Status = ''
        Tester = ''
        Test_Date = ''
        Notes = ''
    })
}

# --- AUTH ---
Add-TC 'AUTH' 'Login' '/auth/login' 'Page Load' 'Verify login page renders' 'App running' 'Open /auth/login' 'Email, password fields and Sign In visible' 'High'
Add-TC 'AUTH' 'Login' '/auth/login' 'Valid Login - Super Admin' 'Super admin can sign in' 'Valid super admin account' 'Enter superadmin@gym.com / SuperAdmin@123; click Sign In' 'Redirect to /super-admin; sidebar visible' 'High'
Add-TC 'AUTH' 'Login' '/auth/login' 'Valid Login - Gym Admin' 'Gym admin can sign in' 'Valid gym admin account' 'Enter admin@fitzone-demo.com / Demo@123; Sign In' 'Redirect to /gym-admin/dashboard' 'High'
Add-TC 'AUTH' 'Login' '/auth/login' 'Invalid Credentials' 'Wrong password rejected' 'None' 'Enter valid email + wrong password; Sign In' 'Error message; stay on login' 'High'
Add-TC 'AUTH' 'Login' '/auth/login' 'Empty Form Validation' 'Required fields enforced' 'None' 'Click Sign In with empty fields' 'Validation errors shown' 'Medium'
Add-TC 'AUTH' 'Login' '/auth/login' 'Show/Hide Password' 'Password visibility toggle' 'On login page' 'Click eye icon on password field' 'Password toggles masked/visible' 'Low'
Add-TC 'AUTH' 'Login' '/auth/login' 'Navigate Forgot Password' 'Link to forgot password' 'On login page' 'Click Forgot Password link' 'Navigate to /auth/forgot-password' 'Medium'
Add-TC 'AUTH' 'Forgot Password' '/auth/forgot-password' 'Submit Email' 'Request password reset' 'On forgot password page' 'Enter registered email; Send Reset Instructions' 'Success message; email flow triggered' 'High'
Add-TC 'AUTH' 'Forgot Password' '/auth/forgot-password' 'Back to Login' 'Return to login' 'On forgot password page' 'Click Back to login' 'Navigate to /auth/login' 'Low'
Add-TC 'AUTH' 'Reset Password' '/auth/reset-password' 'Reset with Token' 'Reset password using token' 'Valid reset token in URL/email' 'Enter email, token, new password, confirm; Reset Password' 'Success; redirect to login' 'High'
Add-TC 'AUTH' 'Reset Password' '/auth/reset-password' 'Password Mismatch' 'Confirm password validation' 'On reset page' 'Enter mismatched passwords; submit' 'Validation error shown' 'Medium'
Add-TC 'AUTH' 'Change Password' '/auth/change-password' 'Forced Change' 'Logged-in user changes password' 'User flagged must-change-password' 'Enter current + new + confirm; Update Password' 'Password updated; access granted' 'High'
Add-TC 'REG' 'Register' '/register' 'Page Load' 'Registration page renders' 'None' 'Open /register' 'Gym signup form visible' 'High'
Add-TC 'REG' 'Register' '/register' 'Valid Registration' 'New gym owner signup' 'Unique email/mobile' 'Fill gym name, owner, mobile, email, address, password; Start free trial' 'Account created; redirect/login success' 'High'
Add-TC 'REG' 'Register' '/register' 'Duplicate Email' 'Duplicate email rejected' 'Email already exists' 'Register with existing email' 'Error message displayed' 'Medium'
Add-TC 'REG' 'Register' '/register' 'Back to Login' 'Navigate to login' 'On register page' 'Click Back to Login' 'Navigate to /auth/login' 'Low'

# --- PUBLIC WEBSITE ---
$pubPages = @(
    @('Home','/website/{slug}','Nav links work','Open public site home','All nav links visible'),
    @('About','/website/{slug}/about','Page load','Click About in nav','About content displayed'),
    @('Plans','/website/{slug}/plans','Plans list','Open Plans page','Membership plans listed'),
    @('Trainers','/website/{slug}/trainers','Trainers list','Open Trainers page','Trainer cards displayed'),
    @('Gallery','/website/{slug}/gallery','Gallery load','Open Gallery page','Images displayed'),
    @('Contact','/website/{slug}/contact','Contact form submit','Fill enquiry form; Send Enquiry','Success confirmation'),
    @('Contact','/website/{slug}/contact','Book trial submit','Fill trial form; Book Trial','Trial booking submitted'),
    @('Home','/website/{slug}','Book Free Trial CTA','Click hero CTA','Navigate to contact/trial section')
)
foreach ($p in $pubPages) {
    Add-TC 'PUB' $p[0] $p[1] $p[2] $p[3] 'Published gym website exists' $p[4] $p[5] 'Medium'
}

# --- SUPER ADMIN ---
$sa = @(
    @('Dashboard','/super-admin','Load','KPI cards and charts render','High'),
    @('Dashboard','/super-admin','Quick Actions','Click quick action links','Navigate to target pages','Medium'),
    @('Gyms','/super-admin/gyms','List Load','Gyms table loads','Gyms listed','High'),
    @('Gyms','/super-admin/gyms','Add Gym','Open Add Gym; fill form; save','New gym appears in list','High'),
    @('Gyms','/super-admin/gyms','Edit Gym','Edit existing gym; save','Changes reflected','High'),
    @('Gyms','/super-admin/gyms','Activate/Deactivate','Toggle gym status','Status badge updates','Medium'),
    @('Gyms','/super-admin/gyms','Delete Gym','Delete gym with confirmation','Gym removed from list','Medium'),
    @('Gym Admins','/super-admin/gym-admins','List Load','Gym admins table loads','Admins listed','High'),
    @('Gym Admins','/super-admin/gym-admins','Add Admin','Create gym admin','Admin created','High'),
    @('Gym Admins','/super-admin/gym-admins','Edit Admin','Edit admin details','Changes saved','High'),
    @('Gym Admins','/super-admin/gym-admins','Resend Temp Password','Click resend temp password','Success notification','Medium'),
    @('Gym Admins','/super-admin/gym-admins','Activate/Deactivate','Toggle admin status','Status updates','Medium'),
    @('Roles','/super-admin/roles','List Load','Roles table loads','Roles listed','High'),
    @('Roles','/super-admin/roles','Add Role','Create new role','Role added','High'),
    @('Roles','/super-admin/roles','Edit Role','Edit role name/details','Changes saved','High'),
    @('Roles','/super-admin/roles','Delete Role','Delete role','Role removed','Medium'),
    @('Privileges','/super-admin/privileges','List Load','Privileges table loads','Privileges listed','High'),
    @('Privileges','/super-admin/privileges','Add Privilege','Create privilege','Privilege added','High'),
    @('Privileges','/super-admin/privileges','Delete Privilege','Delete privilege','Privilege removed','Medium'),
    @('Role Matrix','/super-admin/role-matrix','Matrix Load','Permission matrix renders','Matrix displayed','High'),
    @('Role Matrix','/super-admin/role-matrix','Toggle Permission','Check/uncheck role-privilege cell','Permission assigned/removed','High'),
    @('Audit Logs','/super-admin/audit','List Load','Audit logs load','Logs displayed','High'),
    @('Audit Logs','/super-admin/audit','Search Filter','Search by user/entity/action','Filtered results','Medium'),
    @('Audit Logs','/super-admin/audit','Date Filter','Apply date range filter','Results within range','Medium'),
    @('Audit Logs','/super-admin/audit','Export PDF','Click Export PDF','PDF downloads','Medium'),
    @('Audit Logs','/super-admin/audit','Export Excel','Click Export Excel','Excel downloads','Medium'),
    @('White Label','/super-admin/white-label','Dashboard Load','SaaS white-label dashboard loads','KPIs and table shown','High'),
    @('White Label','/super-admin/white-label','Search','Search premium customers','Filtered table','Medium'),
    @('White Label','/super-admin/white-label','Status Filter','Filter by subscription status','Filtered results','Medium')
)
foreach ($t in $sa) {
    $prio = if ($t.Length -gt 5) { $t[5] } else { 'Medium' }
    Add-TC 'SA' $t[0] $t[1] $t[2] $t[2] 'Logged in as Super Admin' $t[3] $t[4] $prio
}

# --- GYM ADMIN helper ---
function Add-GA($Page, $Route, $Event, $Desc, $Steps, $Expected, $Priority = 'Medium') {
    Add-TC 'GA' $Page $Route $Event $Desc 'Logged in as Gym Admin' $Steps $Expected $Priority
}

# Dashboard & Analytics
Add-GA 'Dashboard' '/gym-admin/dashboard' 'Page Load' 'Dashboard loads with KPIs and charts' 'Open sidebar Dashboard' 'KPI cards and charts visible' 'High'
Add-GA 'Dashboard' '/gym-admin/dashboard' 'Export PDF' 'Export dashboard PDF' 'Click Export PDF' 'PDF file downloads' 'Medium'
Add-GA 'Dashboard' '/gym-admin/dashboard' 'Export Excel' 'Export dashboard Excel' 'Click Export Excel' 'Excel file downloads' 'Medium'
Add-GA 'Dashboard' '/gym-admin/dashboard' 'Quick Nav' 'Quick action strip navigation' 'Click quick nav links' 'Navigate to linked modules' 'Low'
Add-GA 'Revenue Analytics' '/gym-admin/analytics/revenue' 'Page Load' 'Revenue analytics page loads' 'Open Revenue Analytics' 'Charts and KPIs visible' 'High'
Add-GA 'Revenue Analytics' '/gym-admin/analytics/revenue' 'Export' 'Export revenue report' 'Export PDF/Excel' 'Files download' 'Medium'
Add-GA 'Member Analytics' '/gym-admin/analytics/members' 'Page Load' 'Member analytics loads' 'Open Member Analytics' 'Member charts visible' 'High'
Add-GA 'Member Analytics' '/gym-admin/analytics/members' 'Export' 'Export member analytics' 'Export PDF/Excel' 'Files download' 'Medium'
Add-GA 'Attendance Analytics' '/gym-admin/analytics/attendance' 'Page Load' 'Attendance analytics loads' 'Open Attendance Analytics' 'Charts visible' 'High'
Add-GA 'Trainer Analytics' '/gym-admin/analytics/trainers' 'Page Load' 'Trainer analytics loads' 'Open Trainer Analytics' 'Charts visible' 'High'

# Members
Add-GA 'Members List' '/gym-admin/members' 'Page Load' 'Members list loads' 'Open Members' 'Table with members shown' 'High'
Add-GA 'Members List' '/gym-admin/members' 'Search' 'Search members' 'Enter name/email in search' 'Filtered results' 'High'
Add-GA 'Members List' '/gym-admin/members' 'Add Member' 'Create member via dialog' 'Click Add Member; fill form; save' 'Member appears in list' 'High'
Add-GA 'Members List' '/gym-admin/members' 'Edit Member' 'Edit member from row action' 'Click Edit; update; save' 'Changes reflected' 'High'
Add-GA 'Members List' '/gym-admin/members' 'Assign Trainer' 'Assign trainer to member' 'Click Assign trainer; select; save' 'Trainer assigned' 'High'
Add-GA 'Members List' '/gym-admin/members' 'Delete Member' 'Delete/deactivate member' 'Click Delete; confirm' 'Member removed/deactivated' 'Medium'
Add-GA 'Members List' '/gym-admin/members' 'View Detail' 'Navigate to member detail' 'Click member row/view' 'Open /gym-admin/members/:id' 'High'
Add-GA 'Member Detail' '/gym-admin/members/:id' 'Page Load' 'Member detail loads' 'Open member detail' 'Profile info displayed' 'High'
Add-GA 'Member Detail' '/gym-admin/members/:id' 'Edit' 'Edit from detail page' 'Click Edit; save changes' 'Changes saved' 'High'
Add-GA 'Member Detail' '/gym-admin/members/:id' 'View Diet' 'Navigate to member diet' 'Click Diet link' 'Open member diet view' 'Medium'
Add-GA 'Member Detail' '/gym-admin/members/:id' 'View Workout' 'Navigate to member workout' 'Click Workout link' 'Open member workout view' 'Medium'
Add-GA 'Member Diet View' '/gym-admin/members/:id/diet' 'Assign Plan' 'Assign diet plan to member' 'Select plan; assign' 'Plan assigned' 'High'
Add-GA 'Member Diet View' '/gym-admin/members/:id/diet' 'Export PDF' 'Export diet plan PDF' 'Click Export PDF' 'PDF downloads' 'Low'
Add-GA 'Member Workout View' '/gym-admin/members/:id/workout' 'Assign Plan' 'Assign workout plan' 'Select plan; assign' 'Plan assigned' 'High'

# Trainers
Add-GA 'Trainers List' '/gym-admin/trainers' 'Page Load' 'Trainers list loads' 'Open Trainers' 'Table displayed' 'High'
Add-GA 'Trainers List' '/gym-admin/trainers' 'Search' 'Search trainers' 'Enter search text' 'Filtered results' 'Medium'
Add-GA 'Trainers List' '/gym-admin/trainers' 'Add Trainer' 'Create trainer' 'Add Trainer dialog; save' 'Trainer in list' 'High'
Add-GA 'Trainers List' '/gym-admin/trainers' 'Edit Trainer' 'Edit trainer' 'Edit row; save' 'Changes saved' 'High'
Add-GA 'Trainers List' '/gym-admin/trainers' 'Deactivate' 'Deactivate trainer' 'Click Deactivate; confirm' 'Status inactive' 'Medium'
Add-GA 'Trainer Detail' '/gym-admin/trainers/:id' 'Assign Members' 'Assign members to trainer' 'Assign members; save' 'Members linked' 'High'
Add-GA 'Trainer Detail' '/gym-admin/trainers/:id' 'Unassign Member' 'Remove member assignment' 'Unassign from table' 'Member unassigned' 'Medium'

# Leads
Add-GA 'Leads List' '/gym-admin/leads' 'Page Load' 'Leads list loads' 'Open Leads & CRM' 'Leads table/kanban shown' 'High'
Add-GA 'Leads List' '/gym-admin/leads' 'Add Lead' 'Navigate to create lead' 'Click Add Lead' 'Open create form' 'High'
Add-GA 'Leads List' '/gym-admin/leads' 'Search' 'Search leads' 'Enter search' 'Filtered leads' 'Medium'
Add-GA 'Leads List' '/gym-admin/leads' 'Status Filter' 'Filter by status' 'Select status filter' 'Filtered results' 'Medium'
Add-GA 'Leads List' '/gym-admin/leads' 'Kanban Toggle' 'Switch list/kanban view' 'Toggle view mode' 'View switches' 'Low'
Add-GA 'Lead Create' '/gym-admin/leads/create' 'Create Lead' 'Submit new lead form' 'Fill form; Create lead' 'Lead created; redirect/list update' 'High'
Add-GA 'Lead Edit' '/gym-admin/leads/edit/:id' 'Edit Lead' 'Update lead details' 'Modify form; Save changes' 'Lead updated' 'High'
Add-GA 'Lead Detail' '/gym-admin/leads/:id' 'View Detail' 'Lead detail page loads' 'Open lead from list' 'Detail shown' 'High'
Add-GA 'Lead Detail' '/gym-admin/leads/:id' 'Convert to Member' 'Convert lead to member' 'Click Convert; fill modal; submit' 'Member created' 'High'
Add-GA 'Lead Followups' '/gym-admin/leads/followups' 'List Load' 'Pending follow-ups load' 'Open Follow-ups' 'Follow-up list shown' 'Medium'
Add-GA 'Lead Trials' '/gym-admin/leads/trials' 'List Load' 'Today trials load' 'Open Trials' 'Trial list shown' 'Medium'
Add-GA 'Lead Analytics' '/gym-admin/leads/analytics' 'Page Load' 'Lead analytics loads' 'Open Lead Analytics' 'Charts visible' 'High'
Add-GA 'Lead Analytics' '/gym-admin/leads/analytics' 'Export' 'Export lead analytics' 'Export PDF/Excel' 'Files download' 'Medium'

# Memberships & Payments
Add-GA 'Membership Plans' '/gym-admin/membership-plans' 'Page Load' 'Plans list loads' 'Open Membership Plans' 'Plans table shown' 'High'
Add-GA 'Membership Plans' '/gym-admin/membership-plans' 'Create Plan' 'Create membership plan' 'Create plan dialog; save' 'Plan added' 'High'
Add-GA 'Membership Plans' '/gym-admin/membership-plans' 'Edit Plan' 'Edit plan' 'Edit; save' 'Plan updated' 'High'
Add-GA 'Membership Plans' '/gym-admin/membership-plans' 'Deactivate Plan' 'Deactivate plan' 'Deactivate action' 'Plan inactive' 'Medium'
Add-GA 'Memberships' '/gym-admin/memberships' 'Page Load' 'Active memberships load' 'Open Memberships' 'Memberships listed' 'High'
Add-GA 'Memberships' '/gym-admin/memberships' 'Create Membership' 'Assign membership to member' 'Create membership; save' 'Membership created' 'High'
Add-GA 'Memberships' '/gym-admin/memberships' 'Renew' 'Renew membership' 'Click Renew; confirm' 'Membership extended' 'High'
Add-GA 'Memberships' '/gym-admin/memberships' 'Cancel' 'Cancel membership' 'Click Cancel; confirm' 'Membership cancelled' 'Medium'
Add-GA 'Expired Memberships' '/gym-admin/memberships/expired' 'List Load' 'Expired memberships load' 'Open expired link' 'Expired list shown' 'Medium'
Add-GA 'Payments' '/gym-admin/payments' 'Page Load' 'Payments list loads' 'Open Payments' 'Payments table shown' 'High'
Add-GA 'Payments' '/gym-admin/payments' 'Record Payment' 'Record manual payment' 'Record payment dialog; save' 'Payment recorded' 'High'
Add-GA 'Payments' '/gym-admin/payments' 'Download Invoice' 'Download payment invoice' 'Click Download invoice' 'Invoice file downloads' 'Medium'
Add-GA 'Payments' '/gym-admin/payments' 'Refund' 'Refund payment' 'Click Refund; confirm' 'Payment refunded' 'Medium'
Add-GA 'Revenue' '/gym-admin/revenue' 'Page Load' 'Revenue dashboard loads' 'Open Revenue' 'Revenue KPIs shown' 'High'

# Financial
Add-GA 'Expenses' '/gym-admin/expenses' 'Page Load' 'Expenses list loads' 'Open Expenses' 'Expense table shown' 'High'
Add-GA 'Expenses' '/gym-admin/expenses' 'Add Expense' 'Create expense' 'Add expense modal; save' 'Expense added' 'High'
Add-GA 'Expenses' '/gym-admin/expenses' 'Edit Expense' 'Edit expense' 'Edit; save' 'Expense updated' 'High'
Add-GA 'Expenses' '/gym-admin/expenses' 'Delete Expense' 'Delete expense' 'Delete; confirm' 'Expense removed' 'Medium'
Add-GA 'Expenses' '/gym-admin/expenses' 'Export Excel' 'Export expenses' 'Export Excel' 'File downloads' 'Medium'
Add-GA 'Payroll' '/gym-admin/payroll' 'Page Load' 'Payroll list loads' 'Open Payroll' 'Payroll table shown' 'High'
Add-GA 'Payroll' '/gym-admin/payroll' 'Generate Payroll' 'Generate payroll run' 'Generate modal; submit' 'Payroll entries created' 'High'
Add-GA 'Payroll' '/gym-admin/payroll' 'Approve' 'Approve payroll' 'Click Approve' 'Status approved' 'High'
Add-GA 'Payroll' '/gym-admin/payroll' 'Mark Paid' 'Mark payroll paid' 'Click Mark paid' 'Status paid' 'High'
Add-GA 'Payroll' '/gym-admin/payroll' 'Export PDF' 'Export payroll PDF' 'Export PDF' 'File downloads' 'Medium'
Add-GA 'Financial Dashboard' '/gym-admin/financial' 'Page Load' 'Financial dashboard loads' 'Open Financial' 'KPIs/charts shown' 'High'
Add-GA 'Financial Dashboard' '/gym-admin/financial' 'Export' 'Export financial report' 'Export PDF/Excel' 'Files download' 'Medium'

# Branches
Add-GA 'Branches List' '/gym-admin/branches' 'Page Load' 'Branches page loads' 'Open Branches' 'Branch list/form shown' 'High'
Add-GA 'Branches List' '/gym-admin/branches' 'Create Branch' 'Add new branch' 'Fill branch form; Add Branch' 'Branch appears in table' 'High'
Add-GA 'Branches List' '/gym-admin/branches' 'Nav Dashboard' 'Navigate to branch dashboard' 'Click Dashboard link' 'Open branches/dashboard' 'Medium'
Add-GA 'Branch Dashboard' '/gym-admin/branches/dashboard' 'Page Load' 'Branch dashboard loads' 'Open Branch Dashboard' 'Branch KPI cards shown' 'High'
Add-GA 'Branch Dashboard' '/gym-admin/branches/dashboard' 'Sidebar Highlight' 'Only one menu item active' 'Open Branch Dashboard' 'Only Branch Dashboard highlighted' 'Medium'
Add-GA 'Branch Analytics' '/gym-admin/branches/analytics' 'Page Load' 'Branch analytics loads' 'Open Branch Analytics' 'Analytics charts shown' 'High'
Add-GA 'Branch Transfers' '/gym-admin/branches/transfers' 'Transfer Member' 'Transfer member between branches' 'Select member + target branch; Submit' 'Transfer recorded' 'High'
Add-GA 'Branch Transfers' '/gym-admin/branches/transfers' 'Transfer Trainer' 'Transfer trainer between branches' 'Select trainer + branch; Submit' 'Transfer recorded' 'High'
Add-GA 'Branch Targets' '/gym-admin/branches/targets' 'Set Target' 'Save branch monthly target' 'Fill target form; Submit' 'Target saved' 'High'

# Attendance
Add-GA 'Attendance Hub' '/gym-admin/attendance' 'Page Load' 'Attendance dashboard loads' 'Open Attendance' 'Hub links visible' 'High'
Add-GA 'Attendance List' '/gym-admin/attendance/list' 'List Load' 'Attendance records load' 'Open List' 'Records table shown' 'High'
Add-GA 'Check In' '/gym-admin/attendance/check-in' 'Member Check In' 'Check in member' 'Select member; Check In' 'Session opened' 'High'
Add-GA 'Check Out' '/gym-admin/attendance/check-out' 'Member Check Out' 'Check out member' 'Select open session; Check Out' 'Session closed' 'High'
Add-GA 'Member History' '/gym-admin/attendance/members/:id/history' 'History Load' 'Member attendance history' 'Open member history' 'History table shown' 'Medium'
Add-GA 'Member History' '/gym-admin/attendance/members/:id/history' 'Export Excel' 'Export history' 'Export Excel' 'File downloads' 'Low'
Add-GA 'Attendance Reports' '/gym-admin/attendance/reports' 'Daily Report' 'Load daily report' 'Select daily tab; Load' 'Report data shown' 'High'
Add-GA 'Attendance Reports' '/gym-admin/attendance/reports' 'Monthly Report' 'Load monthly report' 'Select monthly tab; Load' 'Report data shown' 'High'
Add-GA 'Attendance Reports' '/gym-admin/attendance/reports' 'Export' 'Export attendance report' 'Export PDF/Excel' 'Files download' 'Medium'
Add-GA 'Trainer Attendance' '/gym-admin/attendance/trainers' 'Trainer Check In' 'Check in trainer' 'Select trainer; Check In' 'Trainer checked in' 'High'
Add-GA 'Trainer Attendance' '/gym-admin/attendance/trainers' 'Trainer Check Out' 'Check out trainer' 'Check Out' 'Trainer checked out' 'High'

# Diet & Workout
Add-GA 'Diet Plans' '/gym-admin/diet-plans' 'Page Load' 'Diet plans list loads' 'Open Diet Plans' 'Plans table shown' 'High'
Add-GA 'Diet Plans' '/gym-admin/diet-plans' 'Search' 'Search diet plans' 'Enter search' 'Filtered plans' 'Medium'
Add-GA 'Diet Plans' '/gym-admin/diet-plans' 'New Plan' 'Navigate to create' 'Click New plan' 'Open editor' 'High'
Add-GA 'Diet Plans' '/gym-admin/diet-plans' 'Assign' 'Assign plan to member' 'Assign action; select member' 'Assignment success' 'High'
Add-GA 'Diet Plans' '/gym-admin/diet-plans' 'Clone' 'Clone diet plan' 'Click Clone' 'Duplicate plan created' 'Medium'
Add-GA 'Diet Plans' '/gym-admin/diet-plans' 'Delete' 'Delete diet plan' 'Delete; confirm' 'Plan removed' 'Medium'
Add-GA 'Diet Plan Editor' '/gym-admin/diet-plans/new' 'Create Plan' 'Save new diet plan' 'Fill plan + items; Save' 'Plan created' 'High'
Add-GA 'Diet Plan Editor' '/gym-admin/diet-plans/:id/edit' 'Edit Plan' 'Update diet plan' 'Modify; Save plan' 'Plan updated' 'High'
Add-GA 'Workout Plans' '/gym-admin/workout-plans' 'Page Load' 'Workout plans list loads' 'Open Workout Plans' 'Plans listed' 'High'
Add-GA 'Workout Plans' '/gym-admin/workout-plans' 'New Plan' 'Create workout plan' 'New Plan; save' 'Plan created' 'High'
Add-GA 'Workout Plans' '/gym-admin/workout-plans' 'Exercise Library Link' 'Open exercise library' 'Click Exercise Library' 'Open exercises page' 'Medium'
Add-GA 'Exercise Library' '/gym-admin/workout-plans/exercises' 'Add Exercise' 'Create exercise' 'Add Exercise; save' 'Exercise added' 'High'
Add-GA 'Exercise Library' '/gym-admin/workout-plans/exercises' 'Edit Exercise' 'Edit exercise' 'Edit; save' 'Exercise updated' 'Medium'
Add-GA 'Exercise Library' '/gym-admin/workout-plans/exercises' 'Delete Exercise' 'Delete exercise' 'Delete; confirm' 'Exercise removed' 'Medium'
Add-GA 'Workout Plan Editor' '/gym-admin/workout-plans/new' 'Create Plan' 'Save workout plan' 'Add exercises; Save' 'Plan created' 'High'
Add-GA 'Workout Plan Editor' '/gym-admin/workout-plans/:id/edit' 'Edit Plan' 'Update workout plan' 'Modify; Save' 'Plan updated' 'High'

# Bookings
Add-GA 'Bookings' '/gym-admin/bookings' 'Page Load' 'Bookings list loads' 'Open Bookings' 'Bookings table shown' 'High'
Add-GA 'Schedules' '/gym-admin/schedules' 'Page Load' 'Class schedules load' 'Open Class Schedules' 'Schedule cards shown' 'High'
Add-GA 'Schedules' '/gym-admin/schedules' 'Cancel Schedule' 'Cancel a schedule' 'Cancel schedule; confirm' 'Schedule cancelled' 'Medium'
Add-GA 'Booking Analytics' '/gym-admin/booking-analytics' 'Page Load' 'Booking analytics loads' 'Open Booking Analytics' 'Analytics shown' 'High'
Add-GA 'Booking Analytics' '/gym-admin/booking-analytics' 'Export' 'Export booking analytics' 'Export PDF/Excel' 'Files download' 'Medium'

# Notifications & Mobile
Add-GA 'Notifications Hub' '/gym-admin/notifications' 'Page Load' 'Notification dashboard loads' 'Open Notifications' 'Quick links shown' 'High'
Add-GA 'Notification Templates' '/gym-admin/notifications/templates' 'List Load' 'Templates load' 'Open Templates' 'Templates listed' 'High'
Add-GA 'Notification Templates' '/gym-admin/notifications/templates' 'Create Template' 'Create template' 'Create; save' 'Template added' 'High'
Add-GA 'Notification Templates' '/gym-admin/notifications/templates' 'Edit Template' 'Edit template' 'Edit; save' 'Template updated' 'Medium'
Add-GA 'Notification Templates' '/gym-admin/notifications/templates' 'Delete Template' 'Delete template' 'Delete; confirm' 'Template removed' 'Medium'
Add-GA 'Notification History' '/gym-admin/notifications/history' 'List Load' 'History loads' 'Open History' 'Sent logs shown' 'Medium'
Add-GA 'Notification Test' '/gym-admin/notifications/test' 'Send Test' 'Send test notification' 'Fill form; Send' 'Test sent success' 'High'
Add-GA 'Mobile Push' '/gym-admin/mobile-notifications' 'Send Push' 'Send mobile push notification' 'Fill form; Send' 'Push sent' 'High'
Add-GA 'Mobile Analytics' '/gym-admin/mobile-analytics' 'Page Load' 'Mobile analytics loads' 'Open Mobile Analytics' 'Stats shown' 'Medium'

# AI, Website, Branding
Add-GA 'AI Dashboard' '/gym-admin/ai' 'Page Load' 'AI dashboard loads' 'Open AI Dashboard' 'Overview shown' 'High'
Add-GA 'AI Insights' '/gym-admin/ai/insights' 'Page Load' 'AI insights loads' 'Open AI Insights' 'Insights/tabs shown' 'High'
Add-GA 'AI Insights' '/gym-admin/ai/insights' 'Tab Switch' 'Switch insights tabs' 'Toggle Insights/Leads tabs' 'Tab content changes' 'Low'
Add-GA 'Website Builder' '/gym-admin/website-builder' 'Page Load' 'Website builder loads' 'Open Website Builder' 'Settings form shown' 'High'
Add-GA 'Website Builder' '/gym-admin/website-builder' 'Save Settings' 'Save site settings' 'Edit settings; Save' 'Settings saved' 'High'
Add-GA 'Website Builder' '/gym-admin/website-builder' 'Publish/Unpublish' 'Toggle publish state' 'Click Publish or Unpublish' 'Status updates' 'High'
Add-GA 'Website Pages' '/gym-admin/website-builder/pages' 'Add Page' 'Add website page' 'Add Page; save' 'Page added' 'High'
Add-GA 'Website Pages' '/gym-admin/website-builder/pages' 'Delete Page' 'Delete page' 'Delete; confirm' 'Page removed' 'Medium'
Add-GA 'Website Gallery' '/gym-admin/website-builder/gallery' 'Add Image' 'Upload gallery image' 'Add Image; save' 'Image added' 'High'
Add-GA 'Website Gallery' '/gym-admin/website-builder/gallery' 'Remove Image' 'Remove gallery image' 'Remove; confirm' 'Image removed' 'Medium'
Add-GA 'Website Testimonials' '/gym-admin/website-builder/testimonials' 'Add Testimonial' 'Add testimonial' 'Fill form; Add' 'Testimonial added' 'High'
Add-GA 'Website Analytics' '/gym-admin/website-builder/analytics' 'Page Load' 'Website analytics loads' 'Open Website Analytics' 'Traffic stats shown' 'Medium'
Add-GA 'White Label Settings' '/gym-admin/white-label' 'Page Load' 'White label settings load' 'Open White Label' 'Branding form shown' 'High'
Add-GA 'White Label Settings' '/gym-admin/white-label' 'Save Branding' 'Save white label settings' 'Edit; Save' 'Settings saved' 'High'
Add-GA 'White Label Preview' '/gym-admin/white-label/preview' 'Preview Load' 'Preview branding' 'Open Preview' 'Preview renders' 'Medium'
Add-GA 'Gym Branding' '/gym-admin/settings/branding' 'Save Branding' 'Save gym logo/receipt branding' 'Upload logo; Save' 'Branding saved' 'High'
Add-GA 'Branding Route' '/gym-admin/branding' 'Page Load' 'Branding alias route loads' 'Open /gym-admin/branding' 'Same as white-label settings' 'Low'
Add-GA 'Subscription' '/gym-admin/subscription' 'Page Load' 'Subscription page loads' 'Open Subscription' 'Plan info shown' 'High'
Add-GA 'Subscription' '/gym-admin/subscription' 'Upgrade Monthly' 'Upgrade to monthly plan' 'Click Upgrade Monthly' 'Subscription updated' 'High'
Add-GA 'Subscription' '/gym-admin/subscription' 'Cancel Subscription' 'Cancel SaaS subscription' 'Click Cancel; confirm' 'Subscription cancelled' 'Medium'
Add-GA 'Audit Logs' '/gym-admin/audit' 'List Load' 'Gym audit logs load' 'Open Audit Logs' 'Logs displayed' 'High'
Add-GA 'Audit Logs' '/gym-admin/audit' 'Filter Search' 'Filter audit logs' 'Apply search/filters' 'Filtered results' 'Medium'
Add-GA 'Audit Logs' '/gym-admin/audit' 'Export' 'Export audit logs' 'Export PDF/Excel' 'Files download' 'Medium'

# --- TRAINER ---
function Add-TR($Page, $Route, $Event, $Desc, $Steps, $Expected, $Priority = 'Medium') {
    Add-TC 'TR' $Page $Route $Event $Desc 'Logged in as Trainer' $Steps $Expected $Priority
}
Add-TR 'Dashboard' '/trainer' 'Page Load' 'Trainer dashboard loads' 'Open Dashboard' 'KPIs and quick links shown' 'High'
Add-TR 'My Members' '/trainer/members' 'List Load' 'Assigned members load' 'Open My Members' 'Members table shown' 'High'
Add-TR 'My Members' '/trainer/members' 'View Workout' 'Open member workout' 'Click View workout' 'Member workout page opens' 'Medium'
Add-TR 'Leads' '/trainer/leads' 'List Load' 'Trainer leads list loads' 'Navigate to /trainer/leads' 'Leads list shown' 'Medium'
Add-TR 'Attendance Hub' '/trainer/attendance' 'Page Load' 'Attendance hub loads' 'Open Attendance' 'Hub links shown' 'High'
Add-TR 'Check In' '/trainer/attendance/check-in' 'Check In Member' 'Trainer checks in member' 'Select member; Check In' 'Check-in success' 'High'
Add-TR 'Check Out' '/trainer/attendance/check-out' 'Check Out Member' 'Trainer checks out member' 'Select session; Check Out' 'Check-out success' 'High'
Add-TR 'Workout Plans' '/trainer/workout-plans' 'List Load' 'Workout plans list' 'Open Workout Plans' 'Plans listed' 'High'
Add-TR 'Member Workout' '/trainer/members/:id/workout' 'Assign Workout' 'Assign workout to member' 'Select plan; assign' 'Plan assigned' 'High'
Add-TR 'AI Recommendations' '/trainer/ai-recommendations' 'List Load' 'AI recommendations load' 'Open AI Recommendations' 'Recommendations listed' 'High'
Add-TR 'AI Recommendations' '/trainer/ai-recommendations' 'Mark Accepted' 'Accept recommendation' 'Click Mark accepted' 'Status updated' 'Medium'
Add-TR 'Schedule' '/trainer/schedule' 'Page Load' 'Trainer schedule loads' 'Open My Schedule' 'Schedule shown' 'High'
Add-TR 'Bookings' '/trainer/bookings' 'List Load' 'Class bookings load' 'Open Class Bookings' 'Bookings listed' 'High'
Add-TR 'Bookings' '/trainer/bookings' 'Search Filter' 'Filter bookings' 'Search/status filter' 'Filtered results' 'Medium'

# --- MEMBER ---
function Add-MB($Page, $Route, $Event, $Desc, $Steps, $Expected, $Priority = 'Medium') {
    Add-TC 'MB' $Page $Route $Event $Desc 'Logged in as Member' $Steps $Expected $Priority
}
Add-MB 'Dashboard' '/member/dashboard' 'Page Load' 'Member dashboard loads' 'Open Dashboard' 'KPIs and quick actions shown' 'High'
Add-MB 'Profile' '/member/profile' 'Page Load' 'Profile page loads' 'Open My Profile' 'Tabs and profile info shown' 'High'
Add-MB 'Profile' '/member/profile' 'Tab Navigation' 'Switch profile tabs' 'Click each tab' 'Tab content loads' 'Medium'
Add-MB 'Profile' '/member/profile' 'Pay Membership Link' 'Navigate to checkout' 'Click Pay Membership' 'Open checkout page' 'High'
Add-MB 'Goals' '/member/goals' 'Add Goal' 'Create fitness goal' 'Fill Add Goal form; submit' 'Goal appears in list' 'High'
Add-MB 'Goals' '/member/goals' 'Mark Complete' 'Complete a goal' 'Click Mark Complete' 'Goal marked complete' 'Medium'
Add-MB 'Progress' '/member/progress' 'Log Progress' 'Log body progress' 'Fill Log Progress form; submit' 'Progress logged' 'High'
Add-MB 'Progress' '/member/progress' 'Export PDF' 'Export progress PDF' 'Export PDF' 'File downloads' 'Low'
Add-MB 'Workouts' '/member/workouts' 'Log Workout' 'Log workout session' 'Fill workout log; submit' 'Workout logged' 'High'
Add-MB 'Workouts' '/member/workouts' 'Mark Complete' 'Mark workout complete' 'Mark Complete' 'Status updated' 'Medium'
Add-MB 'Diets' '/member/diets' 'Log Diet' 'Log daily diet' 'Fill diet log; Log Today' 'Diet logged' 'High'
Add-MB 'Water' '/member/water' 'Save Intake' 'Log water intake' 'Enter amount; Save' 'Intake saved' 'High'
Add-MB 'Referrals' '/member/referrals' 'Copy Code' 'Copy referral code' 'Click Copy Code' 'Code copied to clipboard' 'Medium'
Add-MB 'Feedback' '/member/feedback' 'Submit Feedback' 'Submit gym feedback' 'Rate + comment; Submit Feedback' 'Feedback submitted' 'High'
Add-MB 'My Diet Plan' '/member/diet' 'View Plan' 'View assigned diet plan' 'Open My Diet Plan' 'Plan details shown' 'High'
Add-MB 'My Workout Plan' '/member/workout' 'View Plan' 'View assigned workout plan' 'Open My Workout Plan' 'Plan details shown' 'High'
Add-MB 'Checkout' '/member/checkout' 'Select Plan Pay' 'Pay for membership online' 'Select plan; Pay' 'Payment flow initiated/completed' 'High'
Add-MB 'Bookings' '/member/bookings' 'Book Class' 'Book available class slot' 'Select slot; Book' 'Booking confirmed' 'High'
Add-MB 'Bookings' '/member/bookings' 'Join Waitlist' 'Join class waitlist' 'Click Join Waitlist' 'Added to waitlist' 'Medium'
Add-MB 'Booking History' '/member/bookings/history' 'History Load' 'Past bookings load' 'Open booking history' 'History listed' 'Medium'

# --- CROSS-CUTTING ---
Add-TC 'XCUT' 'Sidebar' 'All portals' 'Menu Navigation' 'Sidebar links navigate correctly' 'Logged in user' 'Click each visible sidebar item' 'Correct page opens' 'High'
Add-TC 'XCUT' 'Sidebar' 'All portals' 'Active Highlight' 'Only relevant menu item highlighted' 'On nested route e.g. branches/dashboard' 'Observe sidebar active state' 'Only child route highlighted' 'Medium'
Add-TC 'XCUT' 'Header' 'All portals' 'Logout' 'User can logout' 'Logged in' 'Open profile menu; Logout' 'Redirect to login; session cleared' 'High'
Add-TC 'XCUT' 'Header' 'All portals' 'Sidenav Toggle' 'Toggle sidebar collapse' 'Logged in' 'Click menu toggle' 'Sidebar opens/closes' 'Low'
Add-TC 'XCUT' 'Auth Guard' 'All protected routes' 'Unauthenticated Access' 'Blocked without login' 'Logged out' 'Open /gym-admin/dashboard directly' 'Redirect to login' 'High'
Add-TC 'XCUT' 'Role Guard' 'Wrong portal' 'Role isolation' 'Trainer cannot access gym-admin' 'Logged in as Trainer' 'Open /gym-admin/dashboard' 'Access denied/redirect' 'High'
Add-TC 'XCUT' 'Permission Guard' 'Restricted route' 'Permission enforcement' 'User lacks permission' 'Login without specific permission' 'Open restricted URL directly' 'Access blocked' 'High'
Add-TC 'XCUT' 'API Errors' 'All data pages' 'Error Toast' 'API failure shows notification' 'Simulate API down/500' 'Load data page' 'Error message shown; no infinite spinner' 'High'
Add-TC 'XCUT' 'Responsive' 'Key pages' 'Mobile Layout' 'Pages usable on mobile width' 'Browser dev tools mobile' 'Open dashboard, members, leads' 'Layout readable; no overlap' 'Low'

# Export CSV
$csvPath = Join-Path $outDir 'MANUAL_TEST_CASES.csv'
$rows | Export-Csv -Path $csvPath -NoTypeInformation -Encoding UTF8

# Build Markdown
$md = @"
# Gym Management System - Manual Test Cases

**Generated:** $(Get-Date -Format 'yyyy-MM-dd')
**Total test cases:** $($rows.Count)

---

## 1. How to Use This Document

| Column | Purpose |
|--------|---------|
| **TC_ID** | Unique test case identifier |
| **Module** | AUTH, REG, PUB, SA, GA, TR, MB, XCUT |
| **Page / Route** | UI page and URL path |
| **Event** | User action being tested |
| **Description** | What this test validates |
| **Preconditions** | Required setup before testing |
| **Steps** | Actions to perform |
| **Expected Result** | Pass criteria |
| **Priority** | High / Medium / Low |
| **Status** | Pass / Fail / Blocked / Not Run |

**Tracking:** Use ``MANUAL_TEST_CASES.csv`` in Excel/Google Sheets to mark Status, Tester, Test_Date, and Notes.

---

## 2. Test Environment

| Item | Value |
|------|-------|
| Frontend URL | http://localhost:4200 |
| Backend API | http://localhost:5088 |
| Browser | Chrome / Edge (latest) |
| Database | GymDb (local SQL Server) |

### Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Super Admin | superadmin@gym.com | SuperAdmin@123 |
| Gym Admin | admin@fitzone-demo.com | Demo@123 |
| Trainer | (demo trainer account) | Demo@123 |
| Member | (demo member account) | Demo@123 |

---

## 3. Module Summary

| Module | Description | Test Cases |
|--------|-------------|------------|
| AUTH | Login, forgot/reset/change password | $($rows | Where-Object Module -eq 'AUTH' | Measure-Object | Select-Object -ExpandProperty Count) |
| REG | Gym owner registration | $($rows | Where-Object Module -eq 'REG' | Measure-Object | Select-Object -ExpandProperty Count) |
| PUB | Public gym website | $($rows | Where-Object Module -eq 'PUB' | Measure-Object | Select-Object -ExpandProperty Count) |
| SA | Super Admin portal | $($rows | Where-Object Module -eq 'SA' | Measure-Object | Select-Object -ExpandProperty Count) |
| GA | Gym Admin portal | $($rows | Where-Object Module -eq 'GA' | Measure-Object | Select-Object -ExpandProperty Count) |
| TR | Trainer portal | $($rows | Where-Object Module -eq 'TR' | Measure-Object | Select-Object -ExpandProperty Count) |
| MB | Member portal | $($rows | Where-Object Module -eq 'MB' | Measure-Object | Select-Object -ExpandProperty Count) |
| XCUT | Cross-cutting (auth, layout, guards) | $($rows | Where-Object Module -eq 'XCUT' | Measure-Object | Select-Object -ExpandProperty Count) |

---

## 4. Test Cases by Module

"@

$currentModule = ''
foreach ($r in $rows) {
    if ($r.Module -ne $currentModule) {
        $currentModule = $r.Module
        $md += "`n### Module: $currentModule`n`n"
    }
    $md += @"
#### $($r.TC_ID) - $($r.Page) - $($r.Event)

| Field | Value |
|-------|-------|
| **Route** | ``$($r.Route)`` |
| **Description** | $($r.Description) |
| **Preconditions** | $($r.Preconditions) |
| **Steps** | $($r.Steps) |
| **Expected Result** | $($r.Expected_Result) |
| **Priority** | $($r.Priority) |
| **Status** | [ ] Not Run |

"@
}

$md += @"

---

## 5. Suggested Test Execution Order

1. **AUTH + REG** — Authentication and registration flows
2. **XCUT** — Guards, logout, sidebar navigation
3. **SA** — Super Admin setup (gyms, admins, roles)
4. **GA Core** — Dashboard, Members, Trainers, Memberships, Payments
5. **GA Operations** — Attendance, Leads, Branches, Financial
6. **GA Advanced** — Diet/Workout, Bookings, Notifications, AI, Website
7. **TR** — Trainer portal workflows
8. **MB** — Member self-service workflows
9. **PUB** — Public website (after website builder publish)

---

## 6. Notes

- Dialog-based CRUD (members, trainers, payments) is tested via list page events.
- ``/gym-admin/branding`` and ``/gym-admin/white-label`` share the same component.
- Trainer ``/trainer/leads`` exists but is not in the trainer sidebar menu.
- Replace ``{slug}`` in public website routes with your gym's published slug.
- For parameterized routes (``:id``), use a valid record ID from your test data.

"@

$mdPath = Join-Path $outDir 'MANUAL_TEST_CASES.md'
[System.IO.File]::WriteAllText($mdPath, $md, [System.Text.UTF8Encoding]::new($true))

Write-Host "Generated $($rows.Count) test cases"
Write-Host "  $mdPath"
Write-Host "  $csvPath"

# 🎯 COMPREHENSIVE PROJECT BRIEF FOR NEW CHAT SESSION

## PROJECT OVERVIEW

**Project Name:** SchoolPC Admin Management System  
**Status:** In Active Development - Bug Fixing Phase  
**Tech Stack:** C# WinForms, .NET 10.0, MySQL 8.0, Microsoft Dependency Injection  
**Database:** MySQL with async operations  
**Current Version:** Beta - UI mostly complete, service layer in progress

---

## SYSTEM ARCHITECTURE

### 1. **Application Structure**

```
SchoolPC.Admin/
├── UI/
│   ├── LoginForm.cs (WinForms designer form - authentication entry point)
│   └── MainShellForm.cs (Main application container with sidebar navigation)
├── Views/
│   ├── PcLogbookListView.cs (Display all PCs in DataGridView)
│   ├── PcLogbookDetailView.cs (Single PC detail view)
│   ├── ProblemReportsView.cs (Display problem reports in DataGridView)
│   └── AccountManagementView.cs (Manage admin accounts)
├── Services/ (Business Logic Layer - Dependency Injected)
│   ├── IPcInstanceService.cs & PcInstanceService.cs
│   ├── IProblemReportService.cs & ProblemReportService.cs
│   ├── IAdminAccountService.cs & AdminAccountService.cs
│   ├── ISessionService.cs & SessionService.cs
│   ├── IStudentAccountService.cs & StudentAccountService.cs
│   ├── PasswordHasher.cs (BCrypt hashing)
│   └── SessionService.cs (Track user sessions)
├── Data/ (Data Access Layer - Repositories)
│   ├── AdminAccountRepository.cs
│   ├── PcInstanceRepository.cs
│   ├── ProblemReportRepository.cs
│   ├── PcGroupRepository.cs
│   ├── StudentAccountRepository.cs
│   └── SessionRepository.cs
├── Models/
│   └── Entities.cs (All data models: AdminAccount, PcInstance, ProblemReport, etc.)
├── Program.cs (Startup code with DI configuration)
└── Database/
	├── database_setup.sql (Initial schema)
	├── migrations/001_pc_groups.sql
	├── scripts/create_admin_account.sql
	└── scripts/rehash_admin_password.sql
```

### 2. **Key Classes & Models**

**AdminAccount.cs:**
```csharp
public class AdminAccount
{
	public string Username { get; set; }
	public string FullName { get; set; }
	public string PasswordHash { get; set; }
	public bool IsActive { get; set; }
	public DateTime CreatedAt { get; set; }
}
```

**PcInstance.cs:**
```csharp
public class PcInstance
{
	public string Guid { get; set; }
	public string ComputerName { get; set; }
	public bool IsActive { get; set; }
	public DateTime LastSeen { get; set; }
	public string GroupId { get; set; }
}
```

**ProblemReport.cs:**
```csharp
public class ProblemReport
{
	public int Id { get; set; }
	public string PcName { get; set; }
	public string StudentName { get; set; }
	public string Message { get; set; }
	public DateTime ReportedAt { get; set; }
	public string Status { get; set; } // "Open" or "Resolved"
	public string ResolvedBy { get; set; }
}
```

---

## CURRENT WORKFLOW / USER FLOW

1. **Login Screen** (LoginForm.cs)
   - User enters admin username/password
   - Password verified using BCrypt hashing
   - On success → Opens MainShellForm with authenticated admin object

2. **Main Dashboard** (MainShellForm.cs)
   - **Sidebar (Left):** Navigation buttons in FlowLayoutPanel
	 - PC Logbook (default)
	 - Problem Reports
	 - Account Management
	 - Logout
   - **Content Area (Right):** Displays current UserControl view
   - Uses SplitContainer with locked splitter (220px fixed sidebar)

3. **PC Logbook View** (PcLogbookListView.cs)
   - Shows all active/inactive PCs in DataGridView
   - Columns: PC Name, Last Seen, Status, Action (button)
   - Click "Action" button → Opens PcLogbookDetailView for that PC

4. **PC Logbook Detail View** (PcLogbookDetailView.cs)
   - Shows detailed info about selected PC
   - **Intended Features** (Shift+F7 to enable editing):
	 - Drag-and-drop PC name field to enable edit mode
	 - Edit PC properties
	 - Save changes back to database
   - Current State: **NOT IMPLEMENTED - Shows white screen**

5. **Problem Reports View** (ProblemReportsView.cs)
   - Shows problem reports from students in DataGridView
   - Columns: PC Name, Student Name, Problem Description, Reported At, Status, Action Button
   - Action Button: "Mark Resolved" to change status
   - Filter by Status dropdown: All / Open / Resolved
   - Current State: **CRASHES WHEN CLICKED - RowStyles index out of range**

6. **Account Management View** (AccountManagementView.cs)
   - Manage admin accounts (CRUD operations)
   - Create new admins, edit, delete
   - Current State: **Basic structure, not tested**

---

## CURRENT ISSUES (CRITICAL - BLOCKING DEVELOPMENT)

### **BUG #1: ProblemReportsView Crashes on Click** 🔴
**Location:** `SchoolPC.Admin\Views\ProblemReportsView.cs` - Line 88  
**Error:** `System.ArgumentOutOfRangeException: Index was out of range`  
**Root Cause:** RowStyles collection not populated with .Add() before accessing by index  
**Affected Lines:**
- Line 88: `mainLayout.RowStyles[1] = new RowStyle(SizeType.Absolute, 50);` ← WRONG
- (Line 57 fixed, but line 88 still uses old pattern)

**Symptom:**
- Navigate to "Problem Reports" tab
- Grid displays with placeholder data successfully
- BUT when Shift+F7 or interaction with grid rows occurs → CRASH

**Fix Required:**
```csharp
// OLD (WRONG - causes crash):
mainLayout.RowStyles[1] = new RowStyle(SizeType.Absolute, 50);

// NEW (CORRECT):
mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50));
```

---

### **BUG #2: ReadOnly DataGridView Blocks Button Clicks** 🟠
**Location:** `SchoolPC.Admin\Views\ProblemReportsView.cs` - Lines 91-98, 120  
**Problem:** 
- Grid has `ReadOnly = true` (line 98)
- But has a button column "Action" (line 120)
- ReadOnly prevents button clicks from firing CellClick event

**Symptom:**
- Button column doesn't respond to clicks
- "Mark Resolved" button is unclickable

**Solutions (Choose 1):**

**Option A:** Change to `ReadOnly = false`
```csharp
_reportsGridView = new DataGridView
{
	// ... other properties ...
	ReadOnly = false,  // ← CHANGE THIS
	// ... other properties ...
};
```

**Option B:** Remove button column, use context menu instead
- Delete line 120
- Implement right-click context menu for "Mark Resolved"

**Recommendation:** Use Option A (simpler)

---

### **BUG #3: Shift+F7 Shows White Screen** 🟠
**Location:** `SchoolPC.Admin\UI\MainShellForm.cs` - Missing keyboard handler  
**Problem:** No keyboard shortcut handler for Shift+F7  
**Intended Feature:** Shift+F7 should enable drag-drop edit mode in PcLogbookDetailView  
**Current State:** Shows blank white screen in content area

**Symptom:**
- Press Shift+F7 in PC Logbook view
- Content area becomes white/blank
- No error message, no visual feedback

**Why It Happens:**
- MainShellForm doesn't have `KeyPreview = true`
- No KeyDown event handler to intercept Shift+F7
- No logic to toggle edit mode

**Fix Required:**
1. Set `KeyPreview = true` in ConfigureUI()
2. Add KeyDown event handler
3. Detect Shift+F7 combination
4. Call method to enable/disable edit mode on current view

---

## DEVELOPMENT GOALS (Priority Order)

### ✅ COMPLETED
- [x] Login system with BCrypt password hashing
- [x] Main application shell with navigation
- [x] PC Logbook list view with DataGridView
- [x] Database schema setup
- [x] Repositories for all entities
- [x] Dependency Injection configuration

### 🔴 CRITICAL BUGS - FIX IMMEDIATELY
1. **Fix BUG #1:** ProblemReportsView RowStyles crash
2. **Fix BUG #2:** ReadOnly property blocking button clicks
3. **Fix BUG #3:** Shift+F7 keyboard shortcut not implemented

### 🟡 IN PROGRESS / INCOMPLETE
- [ ] Problem Reports Service implementation (currently using placeholder data)
- [ ] PC Logbook Detail view edit mode (Shift+F7 functionality)
- [ ] Account Management full CRUD operations
- [ ] Session tracking and timeout
- [ ] Proper dependency injection for all services

### 🔵 FEATURES TO IMPLEMENT AFTER BUGS FIXED
1. **Problem Reports Module:**
   - Fetch real data from database (repo is ready)
   - Mark as resolved → update database
   - Filter by status → async filtering
   - Date range filtering

2. **PC Logbook Detail:**
   - View PC system info, session history
   - Edit mode with Shift+F7
   - Drag-drop PC name to enable editing
   - Save changes to database

3. **Account Management:**
   - List all admin accounts
   - Create new admin (with password hashing)
   - Edit admin info
   - Delete accounts
   - Reset passwords

4. **Session Management:**
   - Track student login/logout sessions per PC
   - Show session logs in PC detail view
   - Generate reports

---

## CODE QUALITY & PATTERNS USED

- **Design Pattern:** Repository Pattern + Service Layer + UI Layer separation
- **Async/Await:** All database calls are async (GetAllActivePcsAsync, etc.)
- **Dependency Injection:** Microsoft.Extensions.DependencyInjection
- **Password Security:** BCrypt.Net-Next for password hashing
- **UI Framework:** Windows Forms (WinForms) with manual layout using panels/tables
- **Logging:** System.Diagnostics.Debug.WriteLine for debug output

---

## DATABASE CONNECTION

**Config:** Stored in code (TODO: move to appsettings.json)  
**Connection String:** MySQL database on localhost  
**Async Support:** MySqlConnector library  
**Current Tables:**
- admin_accounts
- pc_instances
- pc_groups
- problem_reports
- student_accounts
- sessions

---

## NEXT IMMEDIATE ACTIONS

1. **Apply Bug Fixes** (2 files to edit):
   - `SchoolPC.Admin\Views\ProblemReportsView.cs` - Fix RowStyles and ReadOnly
   - `SchoolPC.Admin\UI\MainShellForm.cs` - Add Shift+F7 keyboard handler

2. **Test After Fixes:**
   - Run application
   - Navigate to Problem Reports
   - Click grid rows / buttons (should work)
   - Press Shift+F7 in PC Logbook (should enable edit mode)

3. **After Bugs Fixed:**
   - Connect ProblemReportService to real database data
   - Implement Shift+F7 edit mode with drag-drop functionality
   - Complete Account Management CRUD

---

## FILES TO FOCUS ON

### Critical (Need Immediate Fixes)
- `SchoolPC.Admin\Views\ProblemReportsView.cs` (Lines 56-88, 98, 120)
- `SchoolPC.Admin\UI\MainShellForm.cs` (Lines 1-100, add KeyPreview & KeyDown handler)

### Important (Service Implementation)
- `SchoolPC.Admin\Services\ProblemReportService.cs`
- `SchoolPC.Admin\Views\PcLogbookDetailView.cs` (Edit mode logic)

### Reference (Already Working)
- `SchoolPC.Admin\Data\*` (All repositories working)
- `SchoolPC.Admin\UI\LoginForm.cs` (Authentication working)
- `SchoolPC.Admin\Views\PcLogbookListView.cs` (Display working)

---

## DEBUGGING TIPS

**Debug Output Location:** Visual Studio → Debug → Windows → Output  
All code has Debug.WriteLine() statements prefixed with [SECTION_NAME]

**Example Markers:**
```
[LOGIN] - Authentication flow
[AUTH] - Password verification
[DB] - Database queries
[LoadData] - PC logbook data loading
[ConfigureUI] - UI initialization
```

Watch for these in Output window when bugs occur.

---

## FINAL NOTES

- **Monthly Limit Context:** User reached VS Copilot monthly limit with previous account
- **Urgency:** Project must be finished ASAP
- **Priority:** Fix 3 critical bugs → Connect real data → Implement remaining features
- **No External Libraries:** Using only .NET standard libraries + MySqlConnector + BCrypt.Net

---

## QUICK FIX CHECKLIST

When you resume in new chat, ask to apply these fixes:

- [ ] Fix ProblemReportsView.cs line 56: Change `mainLayout.RowStyles[0]` to `.Add()`
- [ ] Verify line 88: Should be `mainLayout.RowStyles.Add()` not `[1]`
- [ ] Fix line 125: Should be `mainLayout.RowStyles.Add()` not `[2]`
- [ ] Change line 98: `ReadOnly = false` (or implement option B)
- [ ] Add to MainShellForm.cs: `KeyPreview = true` in ConfigureUI()
- [ ] Add KeyDown event handler to catch Shift+F7



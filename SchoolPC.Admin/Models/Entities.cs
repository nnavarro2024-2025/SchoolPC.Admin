namespace SchoolPC.Admin.Models
{
    public class Student
    {
        public string StudentId { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Email { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class AdminAccount
    {
        public int AdminId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "admin";
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }

    public class Session
    {
        public int SessionId { get; set; }
        public string? ClientGuid { get; set; }
        public string PcName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public DateTime LoginTime { get; set; }
        public DateTime? LogoutTime { get; set; }
        public int? DurationMinutes { get; set; }
    }

    public class ProblemReport
    {
        public int ReportId { get; set; }
        public string? ClientGuid { get; set; }
        public string PcName { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Status { get; set; } = "open";
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public string? ResolvedBy { get; set; }
    }

    public class PcInstance
    {
        public int InstanceId { get; set; }
        public string ClientGuid { get; set; } = string.Empty;
        public string FirstPcName { get; set; } = string.Empty;
        public string CurrentPcName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string? Location { get; set; }
        public string? IpAddress { get; set; }
        public bool IsApproved { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime FirstSeen { get; set; }
        public DateTime LastSeen { get; set; }
        public string? Notes { get; set; }
    }

    public class BannedStudent
    {
        public int BanId { get; set; }
        public string StudentId { get; set; } = string.Empty;
        public string? Reason { get; set; }
        public string? BannedBy { get; set; }
        public DateTime BannedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class TestAccount
    {
        public int TestId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; }
    }
}

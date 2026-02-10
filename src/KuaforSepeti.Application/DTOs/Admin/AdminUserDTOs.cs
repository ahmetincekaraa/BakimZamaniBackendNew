namespace KuaforSepeti.Application.DTOs.Admin;

using KuaforSepeti.Domain.Enums;
using KuaforSepeti.Application.DTOs.Common;

public class AdminUserListItem
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginDate { get; set; }
    public int Level { get; set; } // 1: Super, 2: Protected, 3: Standard
    public bool IsProtected { get; set; }
}

public class AdminUserDetail : AdminUserListItem
{
    public int AppointmentCount { get; set; }
    public int ReviewCount { get; set; }
    public string? SalonId { get; set; }
    public string? SalonName { get; set; }
}

public class AdminUserFilterRequest : PaginationRequest
{
    public string? Search { get; set; }
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}

public class UpdateUserRoleRequest
{
    public UserRole Role { get; set; }
}

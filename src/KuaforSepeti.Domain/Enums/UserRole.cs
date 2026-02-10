namespace KuaforSepeti.Domain.Enums;

/// <summary>
/// User roles in the system.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Customer who books appointments.
    /// </summary>
    Customer = 0,

    /// <summary>
    /// Salon owner who manages salon and staff.
    /// </summary>
    SalonOwner = 1,

    /// <summary>
    /// Staff member who provides services.
    /// </summary>
    Staff = 2,

    /// <summary>
    /// System administrator.
    /// </summary>
    Admin = 3
}

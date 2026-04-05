namespace AuthService.Domain.Enums;

/// <summary>
/// Represents the approval and verification status of a user account
/// </summary>
public enum AccountStatus
{
    /// <summary>
    /// Account created but pending admin approval (for RestaurantPartner/Admin)
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Account approved by admin but email not yet verified
    /// </summary>
    Active = 1,

    /// <summary>
    /// Account fully verified and usable
    /// </summary>
    Verified = 2,

    /// <summary>
    /// Account rejected or suspended
    /// </summary>
    Rejected = 3
}

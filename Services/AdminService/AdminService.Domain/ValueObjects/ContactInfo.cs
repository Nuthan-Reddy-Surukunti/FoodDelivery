namespace AdminService.Domain.ValueObjects;

/// <summary>
/// Represents contact information as an immutable value object
/// </summary>
public sealed class ContactInfo
{
    public string Email { get; }
    public string Phone { get; }

    private ContactInfo(string email, string phone)
    {
        Email = email;
        Phone = phone;
    }

    /// <summary>
    /// Creates a new ContactInfo instance with validation
    /// </summary>
    public static ContactInfo Create(string email, string phone)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty", nameof(email));

        if (!IsValidEmail(email))
            throw new ArgumentException("Email format is invalid", nameof(email));

        if (string.IsNullOrWhiteSpace(phone))
            throw new ArgumentException("Phone cannot be empty", nameof(phone));

        if (!IsValidPhone(phone))
            throw new ArgumentException("Phone format is invalid", nameof(phone));

        return new ContactInfo(email, phone);
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }

    private static bool IsValidPhone(string phone)
    {
        // Basic validation: phone should contain only digits, spaces, +, -, (, )
        var cleanedPhone = phone.Replace(" ", "").Replace("-", "").Replace("(", "").Replace(")", "").Replace("+", "");
        return cleanedPhone.Length >= 10 && cleanedPhone.All(char.IsDigit);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not ContactInfo other)
            return false;

        return Email == other.Email && Phone == other.Phone;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Email, Phone);
    }

    public override string ToString()
    {
        return $"Email: {Email}, Phone: {Phone}";
    }

    public static bool operator ==(ContactInfo? left, ContactInfo? right)
    {
        if (ReferenceEquals(left, right))
            return true;

        if (left is null || right is null)
            return false;

        return left.Equals(right);
    }

    public static bool operator !=(ContactInfo? left, ContactInfo? right)
    {
        return !(left == right);
    }
}

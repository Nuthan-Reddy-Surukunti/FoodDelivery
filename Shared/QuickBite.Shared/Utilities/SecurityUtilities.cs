using System;
using System.Security.Cryptography;

namespace QuickBite.Shared.Utilities;

public static class SecurityUtilities
{
    /// <summary>
    /// Generates a cryptographically secure numeric OTP of the specified length.
    /// Default is 6 digits.
    /// </summary>
    public static string GenerateSecureOtp(int length = 6)
    {
        if (length <= 0 || length > 10)
        {
            length = 6; // Fallback for invalid lengths
        }
        
        int min = (int)Math.Pow(10, length - 1);
        int max = (int)Math.Pow(10, length);
        
        return RandomNumberGenerator.GetInt32(min, max).ToString();
    }
}

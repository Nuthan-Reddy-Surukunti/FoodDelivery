using System.Text;

namespace QuickBite.Shared.Utilities;

public static class EmailTemplateBuilder
{
    private const string SupportEmail = "surkuntinuthanreddy@gmail.com";
    private const string PrimaryColor = "#fb7185"; // Soft Rose
    private const string BackgroundColor = "#ffffff";
    private const string CardColor = "#fef2f2";
    private const string TextColor = "#450a0a";
    private const string MutedTextColor = "#991b1b";

    public static string GetOtpEmailTemplate(string name, string otp)
    {
        string body = $@"
            <div style='text-align: center; padding: 20px 0;'>
                <h1 style='color: {PrimaryColor}; margin: 0; font-size: 24px;'>Verification Code</h1>
                <p style='color: {TextColor}; font-size: 16px; margin-top: 10px;'>Hi {name}, use the code below to complete your verification.</p>
                <div style='background: {BackgroundColor}; border: 2px dashed {PrimaryColor}; border-radius: 12px; display: inline-block; padding: 20px 40px; margin: 30px 0;'>
                    <span style='font-family: monospace; font-size: 36px; font-bold: bold; letter-spacing: 8px; color: {PrimaryColor};'>{otp}</span>
                </div>
                <p style='color: {MutedTextColor}; font-size: 14px;'>This code will expire in 10 minutes. If you didn't request this, please ignore this email.</p>
            </div>";

        return GetBaseTemplate("Verify Your Account", body);
    }

    public static string GetAccountApprovedTemplate(string name, string role)
    {
        string body = $@"
            <div style='padding: 20px 0;'>
                <h1 style='color: {PrimaryColor}; margin: 0; font-size: 24px;'>Welcome to QuickBite! 🎉</h1>
                <p style='color: {TextColor}; font-size: 16px; margin-top: 20px;'>
                    Hi {name}, we are excited to inform you that your <b>{role}</b> account has been <b>approved</b> by our administration team.
                </p>
                <p style='color: {TextColor}; font-size: 16px; line-height: 1.6;'>
                    You can now log in to your dashboard to start managing your services. On your first login, you'll receive a verification OTP to activate your account.
                </p>
                <div style='text-align: center; margin-top: 30px;'>
                    <a href='http://localhost:3000/login' style='background: {PrimaryColor}; color: white; padding: 12px 30px; border-radius: 8px; text-decoration: none; font-weight: bold; display: inline-block;'>Log In Now</a>
                </div>
            </div>";

        return GetBaseTemplate("Account Approved", body);
    }

    public static string GetAccountRejectedTemplate(string name, string role, string reason)
    {
        string body = $@"
            <div style='padding: 20px 0;'>
                <h1 style='color: #ef4444; margin: 0; font-size: 24px;'>Account Application Status</h1>
                <p style='color: {TextColor}; font-size: 16px; margin-top: 20px;'>
                    Hi {name}, thank you for your interest in joining QuickBite as a {role}.
                </p>
                <p style='color: {TextColor}; font-size: 16px; line-height: 1.6;'>
                    After reviewing your application, we regret to inform you that we cannot approve your account at this time.
                </p>
                <div style='background: #fee2e2; border-left: 4px solid #ef4444; padding: 15px; margin: 20px 0;'>
                    <p style='color: #991b1b; margin: 0; font-weight: bold;'>Reason for rejection:</p>
                    <p style='color: #b91c1c; margin: 5px 0 0;'>{reason}</p>
                </div>
                <p style='color: {MutedTextColor}; font-size: 14px;'>
                    If you believe this is a mistake or would like to provide additional information, please contact our support team.
                </p>
            </div>";

        return GetBaseTemplate("Account Update", body);
    }

    public static string GetDeliveryAgentAssignedTemplate(string agentName, string orderId, string pickupAddress, string deliveryAddress)
    {
        string body = $@"
            <div style='padding: 20px 0;'>
                <h1 style='color: {PrimaryColor}; margin: 0; font-size: 24px;'>New Delivery Assignment 🛵</h1>
                <p style='color: {TextColor}; font-size: 16px; margin-top: 20px;'>
                    Hi {agentName}, you have been assigned a new delivery!
                </p>
                <div style='background: {BackgroundColor}; border-radius: 12px; padding: 20px; margin: 20px 0;'>
                    <p style='margin: 0 0 10px; color: {MutedTextColor}; font-size: 12px; text-transform: uppercase; font-weight: bold;'>Order ID</p>
                    <p style='margin: 0 0 20px; color: {TextColor}; font-weight: bold; font-family: monospace;'>#{orderId.Split('-')[0].ToUpper()}</p>
                    
                    <p style='margin: 0 0 10px; color: {MutedTextColor}; font-size: 12px; text-transform: uppercase; font-weight: bold;'>Pickup From</p>
                    <p style='margin: 0 0 20px; color: {TextColor};'>{pickupAddress}</p>
                    
                    <p style='margin: 0 0 10px; color: {MutedTextColor}; font-size: 12px; text-transform: uppercase; font-weight: bold;'>Deliver To</p>
                    <p style='margin: 0;'>{deliveryAddress}</p>
                </div>
                <div style='text-align: center; margin-top: 30px;'>
                    <p style='color: {TextColor}; font-size: 14px; margin-bottom: 15px;'>Please proceed to the restaurant for pickup.</p>
                    <a href='http://localhost:3000/agent/dashboard' style='background: {PrimaryColor}; color: white; padding: 12px 30px; border-radius: 8px; text-decoration: none; font-weight: bold; display: inline-block;'>Open Dashboard</a>
                </div>
            </div>";

        return GetBaseTemplate("New Order Assigned", body);
    }

    public static string GetGenericNotificationTemplate(string title, string message, string? ctaText = null, string? ctaUrl = null)
    {
        string body = $@"
            <div style='padding: 20px 0;'>
                <h1 style='color: {PrimaryColor}; margin: 0; font-size: 24px;'>{title}</h1>
                <p style='color: {TextColor}; font-size: 16px; margin-top: 20px; line-height: 1.6;'>
                    {message.Replace("\n", "<br/>")}
                </p>";

        if (!string.IsNullOrEmpty(ctaText) && !string.IsNullOrEmpty(ctaUrl))
        {
            body += $@"
                <div style='text-align: center; margin-top: 30px;'>
                    <a href='{ctaUrl}' style='background: {PrimaryColor}; color: white; padding: 12px 30px; border-radius: 8px; text-decoration: none; font-weight: bold; display: inline-block;'>{ctaText}</a>
                </div>";
        }

        body += "</div>";

        return GetBaseTemplate(title, body);
    }

    private static string GetBaseTemplate(string title, string bodyContent)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>{title}</title>
</head>
<body style='margin: 0; padding: 0; font-family: ""Segoe UI"", Roboto, Helvetica, Arial, sans-serif; background-color: {BackgroundColor};'>
    <table border='0' cellpadding='0' cellspacing='0' width='100%' style='table-layout: fixed;'>
        <tr>
            <td align='center' style='padding: 40px 0;'>
                <table border='0' cellpadding='0' cellspacing='0' width='600' style='background-color: {CardColor}; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);'>
                    <!-- Header/Logo -->
                    <tr>
                        <td align='center' style='padding: 30px 40px 20px; background: linear-gradient(135deg, {PrimaryColor} 0%, #e11d48 100%);'>
                            <span style='color: white; font-size: 32px; font-weight: 800; letter-spacing: -1px;'>QuickBite</span>
                        </td>
                    </tr>
                    <!-- Main Content -->
                    <tr>
                        <td style='padding: 40px;'>
                            {bodyContent}
                        </td>
                    </tr>
                    <!-- Footer -->
                    <tr>
                        <td style='padding: 30px 40px; background-color: #f1f5f9; text-align: center;'>
                            <p style='margin: 0; color: {MutedTextColor}; font-size: 12px;'>
                                &copy; {DateTime.Now.Year} QuickBite Food Delivery. All rights reserved.
                            </p>
                            <p style='margin: 10px 0 0; color: {MutedTextColor}; font-size: 12px;'>
                                Need help? Contact us at <a href='mailto:{SupportEmail}' style='color: {PrimaryColor}; text-decoration: none;'>{SupportEmail}</a>
                            </p>
                            <div style='margin-top: 20px;'>
                                <a href='#' style='display: inline-block; margin: 0 10px; color: {MutedTextColor}; text-decoration: none; font-size: 18px;'>FB</a>
                                <a href='#' style='display: inline-block; margin: 0 10px; color: {MutedTextColor}; text-decoration: none; font-size: 18px;'>TW</a>
                                <a href='#' style='display: inline-block; margin: 0 10px; color: {MutedTextColor}; text-decoration: none; font-size: 18px;'>IG</a>
                            </div>
                        </td>
                    </tr>
                </table>
            </td>
        </tr>
    </table>
</body>
</html>";
    }
}

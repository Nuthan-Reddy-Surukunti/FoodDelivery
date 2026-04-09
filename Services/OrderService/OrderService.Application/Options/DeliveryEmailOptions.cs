namespace OrderService.Application.Options;

public class DeliveryEmailOptions
{
    public const string SectionName = "DeliveryEmail";

    public bool Enabled { get; set; }

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string SenderEmail { get; set; } = string.Empty;

    public string SenderPassword { get; set; } = string.Empty;
}

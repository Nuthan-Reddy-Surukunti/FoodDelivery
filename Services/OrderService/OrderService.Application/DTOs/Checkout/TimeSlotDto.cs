namespace OrderService.Application.DTOs.Checkout;

public class TimeSlotDto
{
    public Guid Id { get; set; }
    
    public string Label { get; set; } = string.Empty;
    
    public int StartMinutesFromNow { get; set; }
    
    public int EndMinutesFromNow { get; set; }
    
    public bool IsAvailable { get; set; }
}

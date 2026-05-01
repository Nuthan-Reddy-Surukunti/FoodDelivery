namespace OrderService.Application.DTOs.Delivery;

public class AgentEarningsSummaryDto
{
    public int TotalDeliveries { get; set; }
    public int TodayDeliveries { get; set; }

    /// <summary>Sum of agent delivery fees across all completed deliveries.</summary>
    public decimal TotalEarnings { get; set; }

    /// <summary>Sum of agent delivery fees for today's completed deliveries.</summary>
    public decimal TodayEarnings { get; set; }

    /// <summary>Total cash collected by agent that needs to be remitted to partners.</summary>
    public decimal TotalRemittance { get; set; }

    public List<AgentDeliveryRecordDto> History { get; set; } = [];
}

public class AgentDeliveryRecordDto
{
    public Guid OrderId { get; set; }
    public string? RestaurantId { get; set; }

    /// <summary>Full customer-facing order total (used for COD remittance display).</summary>
    public decimal OrderTotal { get; set; }

    /// <summary>The agent's actual take-home for this delivery (% of order total, capped).</summary>
    public decimal DeliveryFee { get; set; }

    /// <summary>"CashOnDelivery" or "Online"</summary>
    public string PaymentMethod { get; set; } = "Unknown";

    /// <summary>
    /// For COD orders: the cash the agent collected from the customer and must remit to the platform.
    /// Null for Online orders.
    /// </summary>
    public decimal? CodCashCollected { get; set; }

    public int ItemCount { get; set; }
    public DateTime? DeliveredAt { get; set; }
}

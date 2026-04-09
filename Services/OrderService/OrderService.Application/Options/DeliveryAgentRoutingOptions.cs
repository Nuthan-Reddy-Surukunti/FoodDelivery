namespace OrderService.Application.Options;

public class DeliveryAgentRoutingOptions
{
    public const string SectionName = "DeliveryRouting";

    public int MaxActiveAssignmentsPerAgent { get; set; } = 5;

    public List<DeliveryAgentOption> Agents { get; set; } = [];
}

public class DeliveryAgentOption
{
    public Guid AgentId { get; set; }

    public string AgentName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}

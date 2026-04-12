namespace OrderService.Application.Interfaces;

public interface IDeliveryAgentSyncService
{
    /// <summary>
    /// Syncs all existing DeliveryAgent users from AuthService to OrderService database.
    /// Only syncs agents that don't already exist (prevents duplicates).
    /// Called on application startup and can be triggered manually via admin endpoint.
    /// </summary>
    /// <returns>Number of agents synced</returns>
    Task<int> SyncDeliveryAgentsFromAuthServiceAsync(CancellationToken cancellationToken = default);
}

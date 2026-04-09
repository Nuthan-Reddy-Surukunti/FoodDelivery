namespace OrderService.Domain.Interfaces;

using OrderService.Domain.Entities;

/// <summary>
/// Repository interface for managing delivery agents.
/// </summary>
public interface IDeliveryAgentRepository
{
    /// <summary>
    /// Adds a new delivery agent to the repository.
    /// </summary>
    Task AddAsync(DeliveryAgent agent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a delivery agent by their unique ID.
    /// </summary>
    Task<DeliveryAgent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a delivery agent by their AuthService user ID.
    /// </summary>
    Task<DeliveryAgent?> GetByAuthUserIdAsync(string authUserId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active delivery agents.
    /// </summary>
    Task<IReadOnlyList<DeliveryAgent>> GetAllActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active and email-verified delivery agents.
    /// </summary>
    Task<IReadOnlyList<DeliveryAgent>> GetAllActiveAndVerifiedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing delivery agent.
    /// </summary>
    Task UpdateAsync(DeliveryAgent agent, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a delivery agent from the repository.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

using Microsoft.EntityFrameworkCore;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;
using OrderService.Infrastructure.Data;

namespace OrderService.Infrastructure.Repositories;

/// <summary>
/// Repository implementation for managing delivery agents.
/// </summary>
public class DeliveryAgentRepository : IDeliveryAgentRepository
{
    private readonly OrderDbContext _context;

    public DeliveryAgentRepository(OrderDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(DeliveryAgent agent, CancellationToken cancellationToken = default)
    {
        _context.DeliveryAgents.Add(agent);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<DeliveryAgent?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryAgents
            .FirstOrDefaultAsync(agent => agent.Id == id, cancellationToken);
    }

    public async Task<DeliveryAgent?> GetByAuthUserIdAsync(string authUserId, CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryAgents
            .FirstOrDefaultAsync(agent => agent.AuthUserId == authUserId, cancellationToken);
    }

    public async Task<IReadOnlyList<DeliveryAgent>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryAgents
            .Where(agent => agent.IsActive)
            .OrderBy(agent => agent.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DeliveryAgent>> GetAllActiveAndVerifiedAsync(CancellationToken cancellationToken = default)
    {
        return await _context.DeliveryAgents
            .Where(agent => agent.IsActive && agent.IsEmailVerified)
            .OrderBy(agent => agent.FullName)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(DeliveryAgent agent, CancellationToken cancellationToken = default)
    {
        _context.DeliveryAgents.Update(agent);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var agent = await GetByIdAsync(id, cancellationToken);
        if (agent != null)
        {
            _context.DeliveryAgents.Remove(agent);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

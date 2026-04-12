using Microsoft.Extensions.Logging;
using OrderService.Application.Interfaces;
using OrderService.Domain.Entities;
using OrderService.Domain.Interfaces;
using System.Net.Http.Json;
using System.Text.Json;

namespace OrderService.Application.Services;

public class DeliveryAgentSyncService : IDeliveryAgentSyncService
{
    private readonly IDeliveryAgentRepository _deliveryAgentRepository;
    private readonly HttpClient _httpClient;
    private readonly ILogger<DeliveryAgentSyncService> _logger;

    public DeliveryAgentSyncService(
        IDeliveryAgentRepository deliveryAgentRepository,
        HttpClient httpClient,
        ILogger<DeliveryAgentSyncService> logger)
    {
        _deliveryAgentRepository = deliveryAgentRepository ?? throw new ArgumentNullException(nameof(deliveryAgentRepository));
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<int> SyncDeliveryAgentsFromAuthServiceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Starting delivery agent sync from AuthService...");

            // Call AuthService endpoint to get all delivery agents
            var response = await _httpClient.GetAsync("/api/auth/admin/delivery-agents", cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch delivery agents from AuthService. Status: {StatusCode}", response.StatusCode);
                return 0;
            }

            var agents = await response.Content.ReadFromJsonAsync<List<DeliveryAgentDto>>();
            if (agents == null || agents.Count == 0)
            {
                _logger.LogInformation("No delivery agents found in AuthService.");
                return 0;
            }

            int syncedCount = 0;

            foreach (var agent in agents)
            {
                try
                {
                    // Check if agent already exists in OrderService
                    var existingAgent = await _deliveryAgentRepository.GetByAuthUserIdAsync(agent.UserId, cancellationToken);
                    
                    if (existingAgent != null)
                    {
                        _logger.LogDebug("Agent already synced: {UserId}, Email: {Email}", agent.UserId, agent.Email);
                        continue;
                    }

                    // Create new delivery agent record
                    var deliveryAgent = new DeliveryAgent
                    {
                        Id = Guid.NewGuid(),
                        AuthUserId = agent.UserId,
                        FullName = agent.FullName,
                        Email = agent.Email,
                        IsActive = true,
                        IsEmailVerified = true, // Agents synced from AuthService are considered verified
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _deliveryAgentRepository.AddAsync(deliveryAgent, cancellationToken);
                    syncedCount++;

                    _logger.LogInformation(
                        "Synced delivery agent: UserId={UserId}, Email={Email}, FullName={FullName}",
                        agent.UserId, agent.Email, agent.FullName);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Error syncing delivery agent: UserId={UserId}, Email={Email}",
                        agent.UserId, agent.Email);
                    // Continue with next agent instead of failing entire sync
                }
            }

            _logger.LogInformation("Delivery agent sync completed. Total synced: {SyncedCount}", syncedCount);
            return syncedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Critical error during delivery agent sync from AuthService");
            return 0;
        }
    }
}

/// <summary>
/// DTO for receiving delivery agent data from AuthService
/// </summary>
public class DeliveryAgentDto
{
    public string UserId { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

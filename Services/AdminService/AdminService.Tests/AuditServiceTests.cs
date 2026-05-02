using AdminService.Application.Services;
using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;

namespace AdminService.Tests;

[TestFixture]
public class AuditServiceTests
{
    private FakeAuditLogRepository _repository = null!;
    private AuditService _service = null!;

    [SetUp]
    public void SetUp()
    {
        _repository = new FakeAuditLogRepository();
        _service = new AuditService(_repository);
    }

    [Test]
    public async Task LogActionAsync_ValidInput_WritesAuditRecord()
    {
        var userId = Guid.NewGuid();
        var entityId = Guid.NewGuid();

        await _service.LogActionAsync(
            userId: userId,
            userName: "admin.user",
            action: "Approved",
            entityType: "Restaurant",
            entityId: entityId,
            oldValues: new { Status = "Pending" },
            newValues: new { Status = "Active" },
            ipAddress: "127.0.0.1",
            userAgent: "NUnit");

        var log = _repository.AddedLogs.Single();
        Assert.Multiple(() =>
        {
            Assert.That(log.UserId, Is.EqualTo(userId));
            Assert.That(log.UserName, Is.EqualTo("admin.user"));
            Assert.That(log.Action, Is.EqualTo("Approved"));
            Assert.That(log.EntityType, Is.EqualTo("Restaurant"));
            Assert.That(log.EntityId, Is.EqualTo(entityId));
            Assert.That(log.OldValues, Does.Contain("Pending"));
            Assert.That(log.NewValues, Does.Contain("Active"));
            Assert.That(log.IPAddress, Is.EqualTo("127.0.0.1"));
            Assert.That(log.UserAgent, Is.EqualTo("NUnit"));
        });
    }

    [Test]
    public void LogActionAsync_EmptyUserId_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.LogActionAsync(
                userId: Guid.Empty,
                userName: "admin.user",
                action: "Updated",
                entityType: "Order",
                entityId: Guid.NewGuid()));
    }

    [Test]
    public void LogActionAsync_EmptyAction_ThrowsArgumentException()
    {
        Assert.ThrowsAsync<ArgumentException>(async () =>
            await _service.LogActionAsync(
                userId: Guid.NewGuid(),
                userName: "admin.user",
                action: "",
                entityType: "Order",
                entityId: Guid.NewGuid()));
    }

    [Test]
    public async Task LogStatusChangeAsync_StoresStatusTransitionPayload()
    {
        await _service.LogStatusChangeAsync(
            orderId: Guid.NewGuid(),
            oldStatus: "Pending",
            newStatus: "Cancelled",
            reason: "Customer requested cancellation",
            adminUserId: Guid.NewGuid(),
            adminUserName: "ops.admin");

        var log = _repository.AddedLogs.Single();
        Assert.Multiple(() =>
        {
            Assert.That(log.Action, Is.EqualTo("StatusChanged"));
            Assert.That(log.EntityType, Is.EqualTo("Order"));
            Assert.That(log.OldValues, Does.Contain("Pending"));
            Assert.That(log.NewValues, Does.Contain("Cancelled"));
            Assert.That(log.NewValues, Does.Contain("Customer requested cancellation"));
        });
    }

    [Test]
    public async Task LogApprovalActionAsync_StoresApprovalMetadata()
    {
        await _service.LogApprovalActionAsync(
            entityType: "Restaurant",
            entityId: Guid.NewGuid(),
            action: "Rejected",
            notes: "Documents incomplete",
            adminUserId: Guid.NewGuid(),
            adminUserName: "review.admin");

        var log = _repository.AddedLogs.Single();
        Assert.Multiple(() =>
        {
            Assert.That(log.Action, Is.EqualTo("Rejected"));
            Assert.That(log.EntityType, Is.EqualTo("Restaurant"));
            Assert.That(log.NewValues, Does.Contain("Documents incomplete"));
        });
    }

    private sealed class FakeAuditLogRepository : IAuditLogRepository
    {
        public List<AuditLog> AddedLogs { get; } = new();

        public Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(AddedLogs.FirstOrDefault(x => x.Id == id));

        public Task<IEnumerable<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IEnumerable<AuditLog>>(AddedLogs);

        public Task<AuditLog> AddAsync(AuditLog entity, CancellationToken cancellationToken = default)
        {
            AddedLogs.Add(entity);
            return Task.FromResult(entity);
        }

        public Task UpdateAsync(AuditLog entity, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
            => Task.FromResult(AddedLogs.Any(x => x.Id == id));
    }
}

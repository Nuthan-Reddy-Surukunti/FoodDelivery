using AdminService.Domain.Entities;
using AdminService.Domain.Interfaces;
using AdminService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AdminService.Infrastructure.Repositories
{
    public class AuditLogRepository : IAuditLogRepository
    {
        private readonly AdminServiceDbContext _context;

        public AuditLogRepository(AdminServiceDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<AuditLog> AddAsync(AuditLog entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _context.AuditLogs.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return entity;
        }

        public async Task UpdateAsync(AuditLog entity, CancellationToken cancellationToken = default)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _context.AuditLogs.Update(entity);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var auditLog = await GetByIdAsync(id, cancellationToken);
            if (auditLog != null)
            {
                _context.AuditLogs.Remove(auditLog);
                await _context.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .AnyAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityIdAsync(Guid entityId, CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .Where(a => a.EntityId == entityId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetByEntityTypeAsync(string entityType, CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .Where(a => a.EntityType == entityType)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<AuditLog>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
        {
            return await _context.AuditLogs
                .Where(a => a.Timestamp >= startDate && a.Timestamp <= endDate)
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<(IEnumerable<AuditLog> Items, int TotalCount)> GetPagedAsync(int pageNumber, int pageSize, 
            string? entityType = null, Guid? entityId = null, Guid? userId = null, 
            DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrWhiteSpace(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (entityId.HasValue)
                query = query.Where(a => a.EntityId == entityId.Value);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (startDate.HasValue)
                query = query.Where(a => a.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(a => a.Timestamp <= endDate.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }
    }
}
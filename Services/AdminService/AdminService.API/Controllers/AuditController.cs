using AdminService.Application.DTOs;
using AdminService.Domain.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AdminService.API.Controllers
{
    [ApiController]
    [Route("api/audit")]
    [Authorize(Roles = "Admin")]
    public class AuditController : ControllerBase
    {
        private readonly IAuditLogRepository _auditLogRepository;
        private readonly IMapper _mapper;

        public AuditController(IAuditLogRepository auditLogRepository, IMapper mapper)
        {
            _auditLogRepository = auditLogRepository ?? throw new ArgumentNullException(nameof(auditLogRepository));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        /// <summary>
        /// Get audit logs with optional filtering
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogFilterRequest request)
        {
            try
            {
                if (request == null)
                    request = new AuditLogFilterRequest();

                // Validate pagination
                if (request.PageNumber < 1)
                    request.PageNumber = 1;
                
                if (request.PageSize < 1 || request.PageSize > 100)
                    request.PageSize = 50;

                var (auditLogs, totalCount) = await _auditLogRepository.GetPagedAsync(
                    request.PageNumber,
                    request.PageSize,
                    request.EntityType,
                    request.EntityId,
                    request.UserId,
                    request.StartDate,
                    request.EndDate);

                var auditLogDtos = _mapper.Map<IEnumerable<AuditLogDto>>(auditLogs);

                var result = new PagedResultDto<AuditLogDto>
                {
                    Items = auditLogDtos,
                    TotalCount = totalCount,
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                };

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving audit logs", details = ex.Message });
            }
        }

        /// <summary>
        /// Get specific audit log entry
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuditLog(Guid id)
        {
            try
            {
                var auditLog = await _auditLogRepository.GetByIdAsync(id);
                
                if (auditLog == null)
                    return NotFound(new { message = $"Audit log with ID {id} not found" });

                var auditLogDto = _mapper.Map<AuditLogDto>(auditLog);
                return Ok(auditLogDto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the audit log", details = ex.Message });
            }
        }

        /// <summary>
        /// Get audit trail for a specific entity
        /// </summary>
        [HttpGet("entity/{entityType}/{entityId}")]
        public async Task<IActionResult> GetEntityAuditTrail(string entityType, Guid entityId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(entityType))
                    return BadRequest(new { message = "EntityType is required" });

                if (entityId == Guid.Empty)
                    return BadRequest(new { message = "EntityId is required" });

                var auditLogs = await _auditLogRepository.GetByEntityIdAsync(entityId);
                
                // Filter by entity type for additional safety
                var filteredAuditLogs = auditLogs.Where(a => 
                    string.Equals(a.EntityType, entityType, StringComparison.OrdinalIgnoreCase));

                var auditLogDtos = _mapper.Map<IEnumerable<AuditLogDto>>(filteredAuditLogs);
                return Ok(auditLogDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the entity audit trail", details = ex.Message });
            }
        }

        /// <summary>
        /// Get audit logs for a specific user
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserAuditLogs(Guid userId)
        {
            try
            {
                if (userId == Guid.Empty)
                    return BadRequest(new { message = "UserId is required" });

                var auditLogs = await _auditLogRepository.GetByUserIdAsync(userId);
                var auditLogDtos = _mapper.Map<IEnumerable<AuditLogDto>>(auditLogs);
                
                return Ok(auditLogDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving user audit logs", details = ex.Message });
            }
        }
    }
}
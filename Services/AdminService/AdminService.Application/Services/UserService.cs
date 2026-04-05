using AutoMapper;
using AdminService.Application.DTOs.Requests;
using AdminService.Application.DTOs.Responses;
using AdminService.Application.Interfaces;
using AdminService.Domain.Entities;
using AdminService.Domain.Enums;
using AdminService.Domain.Interfaces;
using AdminService.Domain.ValueObjects;

namespace AdminService.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IMapper _mapper;

    public UserService(IUserRepository userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<UserDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {id} not found");

        return _mapper.Map<UserDto>(user);
    }

    public async Task<PagedResultDto<UserDto>> GetAllAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        var (users, totalCount) = await _userRepository.GetPagedAsync(pageNumber, pageSize, cancellationToken);
        
        return new PagedResultDto<UserDto>
        {
            Items = _mapper.Map<IEnumerable<UserDto>>(users),
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<IEnumerable<UserDto>> GetByRoleAsync(string role, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<UserRole>(role, out var userRole))
            throw new ArgumentException($"Invalid role: {role}");

        var users = await _userRepository.GetByRoleAsync(userRole, cancellationToken);
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        if (!Enum.TryParse<UserRole>(request.Role, out var userRole))
            throw new ArgumentException($"Invalid role: {request.Role}");

        var contactInfo = ContactInfo.Create(request.Email, request.Phone);
        var user = User.Create(request.Email, request.Password, userRole, contactInfo);

        var createdUser = await _userRepository.AddAsync(user, cancellationToken);
        return _mapper.Map<UserDto>(createdUser);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(id, cancellationToken);
        if (user == null)
            throw new KeyNotFoundException($"User with ID {id} not found");

        if (request.Phone != null || request.Email != null)
        {
            var email = request.Email ?? ((User)user).ContactInfo.Email;
            var phone = request.Phone ?? ((User)user).ContactInfo.Phone;
            var newContactInfo = ContactInfo.Create(email, phone);
            ((User)user).UpdateContactInfo(newContactInfo);
        }

        if (request.Role != null && Enum.TryParse<UserRole>(request.Role, out var newRole))
        {
            ((User)user).ChangeRole(newRole);
        }

        if (request.IsActive.HasValue)
        {
            if (request.IsActive.Value)
                ((User)user).Activate();
            else
                ((User)user).Deactivate();
        }

        await _userRepository.UpdateAsync(user, cancellationToken);
        return _mapper.Map<UserDto>(user);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var exists = await _userRepository.ExistsAsync(id, cancellationToken);
        if (!exists)
            throw new KeyNotFoundException($"User with ID {id} not found");

        await _userRepository.DeleteAsync(id, cancellationToken);
    }
}

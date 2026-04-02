using System;
using AuthService.Domain.Entities;

namespace AuthService.Domain.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}

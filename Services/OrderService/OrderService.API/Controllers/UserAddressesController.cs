using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderService.API.Utilities;
using OrderService.Application.DTOs.Requests;
using OrderService.Application.Interfaces;

namespace OrderService.API.Controllers;

[ApiController]
[Route("gateway/user/addresses")]
[Authorize(Roles = "Customer")]
public class UserAddressesController : ControllerBase
{
    private readonly IUserAddressService _userAddressService;

    public UserAddressesController(IUserAddressService userAddressService)
    {
        _userAddressService = userAddressService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAddresses(CancellationToken cancellationToken)
    {
        var currentUserId = this.GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var addresses = await _userAddressService.GetUserAddressesAsync(currentUserId, cancellationToken);
        return Ok(addresses);
    }

    [HttpGet("{addressId:guid}")]
    public async Task<IActionResult> GetAddressById([FromRoute] Guid addressId, CancellationToken cancellationToken)
    {
        var currentUserId = this.GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var address = await _userAddressService.GetUserAddressByIdAsync(currentUserId, addressId, cancellationToken);
        return Ok(address);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAddress([FromBody] CreateUserAddressRequestDto request, CancellationToken cancellationToken)
    {
        var currentUserId = this.GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var created = await _userAddressService.CreateUserAddressAsync(currentUserId, request, cancellationToken);
        return CreatedAtAction(nameof(GetAddressById), new { addressId = created.AddressId }, created);
    }

    [HttpPut("{addressId:guid}")]
    public async Task<IActionResult> UpdateAddress(
        [FromRoute] Guid addressId,
        [FromBody] UpdateUserAddressRequestDto request,
        CancellationToken cancellationToken)
    {
        var currentUserId = this.GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        var updated = await _userAddressService.UpdateUserAddressAsync(currentUserId, addressId, request, cancellationToken);
        return Ok(updated);
    }

    [HttpDelete("{addressId:guid}")]
    public async Task<IActionResult> DeleteAddress([FromRoute] Guid addressId, CancellationToken cancellationToken)
    {
        var currentUserId = this.GetCurrentUserId();
        if (currentUserId == Guid.Empty)
        {
            return Unauthorized();
        }

        await _userAddressService.DeleteUserAddressAsync(currentUserId, addressId, cancellationToken);
        return NoContent();
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using motomart_BE.DTOs;
using motomart_BE.Models;
using motomart_BE.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace motomart_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;
        private readonly IUserService _userService;

        public AddressesController(IAddressService addressService, IUserService userService)
        {
            _addressService = addressService;
            _userService = userService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAddresses()
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var addresses = await _addressService.GetAddressesByUserId(userId);
            return Ok(addresses.Select(a => MapToDto(a)));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAddress(Guid id)
        {
            var userId = GetCurrentUserId();
            var address = await _addressService.GetAddressById(id);

            if (address == null) return NotFound();
            if (address.UserId != userId) return Forbid();

            return Ok(MapToDto(address));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAddress([FromBody] AddressCreateDto createDto)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var address = new Address
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Street = createDto.Street,
                City = createDto.City,
                State = createDto.State,
                PostalCode = createDto.PostalCode,
                Country = createDto.Country,
                IsDefault = createDto.IsDefault
            };

            var createdAddress = await _addressService.CreateAddress(address);
            return CreatedAtAction(nameof(GetAddress), new { id = createdAddress.Id }, MapToDto(createdAddress));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAddress(Guid id, [FromBody] AddressCreateDto updateDto)
        {
            var userId = GetCurrentUserId();
            var address = await _addressService.GetAddressById(id);

            if (address == null) return NotFound();
            if (address.UserId != userId) return Forbid();

            address.Street = updateDto.Street;
            address.City = updateDto.City;
            address.State = updateDto.State;
            address.PostalCode = updateDto.PostalCode;
            address.Country = updateDto.Country;
            address.IsDefault = updateDto.IsDefault;

            var updatedAddress = await _addressService.UpdateAddress(address);
            return Ok(MapToDto(updatedAddress));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAddress(Guid id)
        {
            var userId = GetCurrentUserId();
            var address = await _addressService.GetAddressById(id);

            if (address == null) return NotFound();
            if (address.UserId != userId) return Forbid();

            await _addressService.DeleteAddress(id);
            return NoContent();
        }

        [HttpPatch("{id}/default")]
        public async Task<IActionResult> SetDefault(Guid id)
        {
            var userId = GetCurrentUserId();
            if (userId == Guid.Empty) return Unauthorized();

            var success = await _addressService.SetDefaultAddress(userId, id);
            if (!success) return NotFound();

            return NoContent();
        }

        private Guid GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return Guid.Empty;
            return Guid.Parse(userIdClaim);
        }

        private static AddressDto MapToDto(Address address)
        {
            return new AddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                Street = address.Street,
                City = address.City,
                State = address.State,
                PostalCode = address.PostalCode,
                Country = address.Country,
                IsDefault = address.IsDefault
            };
        }
    }
}

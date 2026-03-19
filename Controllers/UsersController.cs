using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using motomart_BE.DTOs;
using motomart_BE.Models;
using motomart_BE.Services;
using System.Security.Claims;

namespace motomart_BE.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUsers([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null)
        {
            var pagedList = await _userService.GetUsers(pageNumber, pageSize, searchTerm);
            
            var userDtos = pagedList.Select(u => new UserResponseDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                Role = u.Role,
                CreatedAt = u.CreatedAt
            }).ToList();

            var result = new
            {
                items = userDtos,
                currentPage = pagedList.CurrentPage,
                totalPages = pagedList.TotalPages,
                pageSize = pagedList.PageSize,
                totalCount = pagedList.TotalCount,
                hasPrevious = pagedList.HasPrevious,
                hasNext = pagedList.HasNext
            };

            return Ok(result);
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(email)) return Unauthorized();

            var user = await _userService.GetUserByEmail(email);
            if (user == null) return NotFound("User not found.");

            return Ok(new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UserUpdateDto updateDto)
        {
            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            var currentUserRole = User.FindFirst(ClaimTypes.Role)?.Value;

            var userToUpdate = await _userService.GetUserById(id);
            if (userToUpdate == null) return NotFound("User not found.");

            // Check if user is Admin OR updating their own profile
            if (currentUserRole != "Admin" && userToUpdate.Email != currentUserEmail)
            {
                return Forbid();
            }

            userToUpdate.Name = updateDto.Name;
            userToUpdate.Email = updateDto.Email;
            userToUpdate.PhoneNumber = updateDto.PhoneNumber;

            var updatedUser = await _userService.UpdateUser(userToUpdate);

            return Ok(new UserResponseDto
            {
                Id = updatedUser.Id,
                Name = updatedUser.Name,
                Email = updatedUser.Email,
                PhoneNumber = updatedUser.PhoneNumber,
                Role = updatedUser.Role,
                CreatedAt = updatedUser.CreatedAt
            });
        }
    }
}

using System.Diagnostics;
using System.Security.Cryptography;
using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using motomart_BE.Data;
using motomart_BE.DTOs;
using motomart_BE.Helpers;
using motomart_BE.Models;

namespace motomart_BE.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
        Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto);
        Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto);
    }

    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IJwtHelper _jwtHelper;
        private readonly IEmailService _emailService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(AppDbContext context, IJwtHelper jwtHelper, IEmailService emailService, ILogger<AuthService> logger)
        {
            _context = context;
            _jwtHelper = jwtHelper;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting registration process for {Email}", registerDto.Email);

            try
            {
                // Check if user already exists
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == registerDto.Email);

                if (existingUser != null)
                {
                    _logger.LogWarning("Registration failed: User with email {Email} already exists.", registerDto.Email);
                    throw new Exception("User already exists.");
                }

                var user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = registerDto.Name,
                    Email = registerDto.Email,
                    PhoneNumber = registerDto.PhoneNumber,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password),
                    Role = "User", // Default role
                    CreatedAt = DateTime.UtcNow
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                stopwatch.Stop();
                _logger.LogInformation("Registration successful for {Email} in {Elapsed}ms", registerDto.Email, stopwatch.ElapsedMilliseconds);

                return new AuthResponseDto
                {
                    Token = _jwtHelper.GenerateToken(user),
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Registration error for {Email} after {Elapsed}ms: {Message}", registerDto.Email, stopwatch.ElapsedMilliseconds, ex.Message);
                throw;
            }
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
        {
            var stopwatch = Stopwatch.StartNew();
            _logger.LogInformation("Starting login process for {Email}", loginDto.Email);

            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == loginDto.Email);

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login failed for {Email}: Invalid credentials.", loginDto.Email);
                    throw new Exception("Invalid email or password.");
                }

                stopwatch.Stop();
                _logger.LogInformation("Login successful for {Email} in {Elapsed}ms", loginDto.Email, stopwatch.ElapsedMilliseconds);

                return new AuthResponseDto
                {
                    Token = _jwtHelper.GenerateToken(user),
                    Name = user.Name,
                    Email = user.Email,
                    Role = user.Role
                };
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(ex, "Login error for {Email} after {Elapsed}ms: {Message}", loginDto.Email, stopwatch.ElapsedMilliseconds, ex.Message);
                throw;
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDto forgotPasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == forgotPasswordDto.Email);
            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                return true;
            }

            user.ResetToken = Convert.ToHexString(RandomNumberGenerator.GetBytes(64));
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour

            await _context.SaveChangesAsync();

            var resetLink = $"http://localhost:4200/reset-password?token={user.ResetToken}";
            var emailBody = $"Please reset your password by clicking here: <a href='{resetLink}'>{resetLink}</a>";

            await _emailService.SendEmailAsync(user.Email, "Reset Your Password", emailBody);

            return true;
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDto resetPasswordDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(
                u => u.ResetToken == resetPasswordDto.Token && u.ResetTokenExpiry > DateTime.UtcNow);

            if (user == null)
            {
                throw new Exception("Invalid or expired password reset token.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(resetPasswordDto.NewPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}

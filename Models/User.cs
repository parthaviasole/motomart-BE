using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace motomart_BE.Models
{
    [System.ComponentModel.DataAnnotations.Schema.Table("users")]
    public class User
    {
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column("id")]
        public Guid Id { get; set; }

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column("name")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [System.ComponentModel.DataAnnotations.Schema.Column("email")]
        public string Email { get; set; } = string.Empty;

        [System.ComponentModel.DataAnnotations.Schema.Column("phone_number")]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("role")]
        public string Role { get; set; } = "User";

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("reset_token")]
        public string? ResetToken { get; set; }

        [Column("reset_token_expiry")]
        public DateTime? ResetTokenExpiry { get; set; }
    }

    [System.ComponentModel.DataAnnotations.Schema.Table("addresses")]
    public class Address
    {
        [Key]
        [System.ComponentModel.DataAnnotations.Schema.Column("id")]
        public Guid Id { get; set; }

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column("user_id")]
        public Guid UserId { get; set; }

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column("street")]
        public string Street { get; set; } = string.Empty;

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column("city")]
        public string City { get; set; } = string.Empty;

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column("state")]
        public string State { get; set; } = string.Empty;

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column("postal_code")]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [System.ComponentModel.DataAnnotations.Schema.Column("country")]
        public string Country { get; set; } = string.Empty;

        [Column("is_default")]
        public bool IsDefault { get; set; } = false;
    }
}

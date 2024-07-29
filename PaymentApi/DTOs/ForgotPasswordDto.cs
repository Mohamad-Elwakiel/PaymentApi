using System.ComponentModel.DataAnnotations;

namespace PaymentApi.DTOs
{
    public class ForgotPasswordDto
    {
        [Required, EmailAddress]
        public  string? Email { get; set; }
        [Required]
        public string? clientUri { get; set;}
    }
}

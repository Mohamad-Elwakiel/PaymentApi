using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace PaymentApi.DTOs
{
    public class LoginModelDto
    {
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }
}

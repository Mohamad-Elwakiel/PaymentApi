using Microsoft.AspNetCore.Identity;
using PaymentApi.DTOs;
using PaymentApi.Models;

namespace PaymentApi.Repositorey
{
    public interface IAccountRepository
    {
        Task<IdentityResult> SignUpAsync(SignUpModel signUpModel);
        Task<string> LoginAsync(LoginModel loginModel);
        
    }
}

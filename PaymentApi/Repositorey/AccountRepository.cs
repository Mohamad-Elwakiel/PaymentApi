using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using NuGet.Packaging.Signing;
using PaymentApi.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;
using Azure.Core;
using PaymentApi.DTOs;
using Microsoft.AspNetCore.WebUtilities;
using EmailService;
namespace PaymentApi.Repositorey
{
    public class AccountRepository : IAccountRepository
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly IEmailSender _emailSender;
        public AccountRepository(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager,
            IConfiguration config, IEmailSender emailSender)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _emailSender = emailSender;
        }
        public async Task<IdentityResult> SignUpAsync(SignUpModel signUpModel)
        {

            var user = new ApplicationUser()
            {
                FirstName = signUpModel.FirstName,
                LastName = signUpModel.LastName,
                Email = signUpModel.Email,
                DateOfBirth = signUpModel.DateOfBirth,
                UserName = signUpModel.Email,

            };
            var result = await _userManager.CreateAsync(user, signUpModel.Password);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(Environment.NewLine, result.Errors.Select(e => e.Description)));
            }
            return result;
        }
        public async Task<string> LoginAsync(LoginModel loginModel)
        {
            var result = await _signInManager.PasswordSignInAsync(loginModel.Email, loginModel.Password,
                false, false);
            if (!result.Succeeded)
            {
                return null;
            }
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, loginModel.Email),
                //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            /* var user = await _userManager.FindByEmailAsync(loginModel.Email);
             var roles = await _userManager.GetRolesAsync(user);
             foreach(var role in roles)
             {
                 authClaims.Add(new Claim(ClaimTypes.Role, role));
             }*/
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisismysecretkey123456789145678946thisismysecret"));
            var token = new JwtSecurityToken(
                issuer: _config["JWT:ValidIssuer"],
                audience: _config["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)

                );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}

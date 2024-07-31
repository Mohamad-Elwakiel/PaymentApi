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
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
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
        public async Task<LoginResponse> LoginAsync(LoginModel loginModel)
        {
            var response = new LoginResponse();
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
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            /*
             var roles = await _userManager.GetRolesAsync(user);
             foreach(var role in roles)
             {
                 authClaims.Add(new Claim(ClaimTypes.Role, role));
             }*/
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisismysecretkey123456789145678946thisismysecret"));
            var token = new JwtSecurityToken(
                issuer: _config["JWT:ValidIssuer"],
                audience: _config["JWT:ValidAudience"],
                expires: DateTime.Now.AddSeconds(60),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)

                );
            response.IsLoggedIn = true;

            response.JwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            response.RefreshToken = this.GenerateRefreshToken();
            user.RefreshToken = response.RefreshToken;
            user.RefreshTokenExpiryTime = DateTime.Now.AddHours(12);
            await _userManager.UpdateAsync(user);
            return response;
        }
        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenModel refreshToken)
        {
            var principale = this.GetTokenPrincipal(refreshToken.JwtToken);

            var response = new LoginResponse();
            if(principale?.Identity?.Name is null)
            {
                return response;
            }
            
            var identityUser = await _userManager.FindByNameAsync(principale.Identity.Name);
            if(identityUser is null || identityUser.RefreshToken != refreshToken.RefreshToken || 
                identityUser.RefreshTokenExpiryTime > DateTime.Now)
            {
                return response;
            }
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, identityUser.Email),
                //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            };
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisismysecretkey123456789145678946thisismysecret"));
            var token = new JwtSecurityToken(
                issuer: _config["JWT:ValidIssuer"],
                audience: _config["JWT:ValidAudience"],
                expires: DateTime.Now.AddSeconds(60),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)

                );
            response.IsLoggedIn = true;
            response.JwtToken = new JwtSecurityTokenHandler().WriteToken(token);
            response.RefreshToken = this.GenerateRefreshToken();
            identityUser.RefreshToken = response.RefreshToken;
            identityUser.RefreshTokenExpiryTime = DateTime.Now.AddHours(12);
            await _userManager.UpdateAsync(identityUser);
            return response;



        }
        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
                
            }
            return Convert.ToBase64String(randomNumber);
        }
        private ClaimsPrincipal? GetTokenPrincipal(string token)
        {
            var securityKey  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisismysecretkey123456789145678946thisismysecret"));
            var validation = new TokenValidationParameters
            {
                IssuerSigningKey = securityKey,
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = _config.GetSection("JWT:ValidAudience").Value,
                ValidIssuer = _config.GetSection("JWT:ValidIssuer").Value,



            };
            return new JwtSecurityTokenHandler().ValidateToken(token, validation, out var validatedToken);
        }
    }
}

using AutoMapper;
using EmailService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NETCore.MailKit.Core;
using PaymentApi.DTOs;
using PaymentApi.Models;
using PaymentApi.Repositorey;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PaymentApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountRepository _accountRepository;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        public AccountController(IAccountRepository accountRepository, IMapper mapper, IEmailSender emailSender,
            UserManager<ApplicationUser> userManager, IConfiguration config)
        {
            _accountRepository = accountRepository;
            _mapper = mapper;
            _emailSender = emailSender;
            _userManager = userManager;
            _config = config;
        }
        [HttpPost("register")]
        public async Task<IActionResult> SignUp([FromBody] SignUpModelDto signUpModelDto)
        {
            var signUpModel = _mapper.Map<SignUpModel>(signUpModelDto);
            var result = await _accountRepository.SignUpAsync(signUpModel);
            if (result.Succeeded)
            {
                return Ok(result.Succeeded);
            }
            return Unauthorized();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDto loginModelDto)
        {
            var loginModel = _mapper.Map<LoginModel>(loginModelDto);
            var result = await _accountRepository.LoginAsync(loginModel);
            if (result==null)
            {
                return Unauthorized();
            }
           
            return Ok(result);

        }
        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel refreshToken)
        {
            var result = await _accountRepository.RefreshTokenAsync(refreshToken);
            if (result == null)
            {
                return Unauthorized();
            }
            return Ok(result);
        }
        [HttpPost("ForgotPassword")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var user = await _userManager.FindByEmailAsync(forgotPassword.Email!);
            if (user == null)
            {
                return Unauthorized();
            }
            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, forgotPassword.Email),
                //new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("thisismysecretkey123456789145678946thisismysecret"));
            var token = new JwtSecurityToken(
                issuer: _config["JWT:ValidIssuer"],
                audience: _config["JWT:ValidAudience"],
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)

                );
            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            //var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var param = new Dictionary<string, string?>
            {
                { "token", jwtToken},
                { "email", forgotPassword.Email }
            };
            var callback = QueryHelpers.AddQueryString(forgotPassword.clientUri!, param);
            var message = new Message([user.Email], "Reset Password", callback);
            await _emailSender.sendEmailAsync(message);
            return Ok();
        }
        [HttpPost("ResetPassword")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto resetPassword)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            var user = await _userManager.FindByEmailAsync(resetPassword.Email!);
            if (user == null)
            {
                return BadRequest("Invalid Request");
            }

            var result = await _userManager.ResetPasswordAsync(user, resetPassword.Token, resetPassword.Password);

            // verfy token using jwt

            var isTokenValid = new JwtSecurityTokenHandler().ValidateTokenAsync(resetPassword.Token, new TokenValidationParameters { });
            if (isTokenValid.IsCompletedSuccessfully)
            {
                // reset password
                await _userManager.RemovePasswordAsync(user);
                await _userManager.AddPasswordAsync(user, resetPassword.Password);

                return Ok("Password Reset Successful");
            }
            else
            {
                return BadRequest("Invalid Token");
            }

            
        }

    }

}

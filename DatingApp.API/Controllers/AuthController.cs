using System.Threading.Tasks;
using DatingApp.API.Data;
using Microsoft.AspNetCore.Mvc;
using DatingApp.API.Models;
using DatingApp.API.DTOs;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System;
using System.IdentityModel.Tokens.Jwt;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController: ControllerBase
    {
        private readonly IAuthRepository authRepository;
        private readonly IConfiguration _config;
        public AuthController(IAuthRepository authRepository, IConfiguration config)
        {
            this.authRepository = authRepository;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(UserRegister userRegister)
        {
            userRegister.UserName = userRegister.UserName.ToLower();

            if(await this.authRepository.UserExists(userRegister.UserName))
            {
                return BadRequest("User Name Already exists.");        
            }

            var user = new User{
                UserName = userRegister.UserName,
                FirstName= userRegister.FirstName,
                LastName = userRegister.LastName
            };
            var createdUser = await this.authRepository.Register(user,userRegister.Password);
            return StatusCode(201);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserLogin userLogin)
        {
            var user = await this.authRepository.Login(userLogin.UserName, userLogin.Password);
            if(user == null)
            {
                return Unauthorized();
            }

            var claims = new []
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("AppSettings:Token").Value));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);
            
            var tokenDescriptor = new SecurityTokenDescriptor{
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new {
                token = tokenHandler.WriteToken(token)       
            });
        }
    }
}
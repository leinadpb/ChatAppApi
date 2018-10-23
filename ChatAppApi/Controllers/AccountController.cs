using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using ChatAppApi.Services;
using ChatAppApi.Models;
using Microsoft.Extensions.Configuration;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Newtonsoft.Json;
using JWT.Builder;
using JWT;

namespace ChatAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<UserApp> _userManager;
        private readonly SignInManager<UserApp> _signManager;
        private readonly IConfiguration _config;

        public AccountController(MessagesService srv,
           UserManager<UserApp> userManager, SignInManager<UserApp> signManager,
           IConfiguration cofig)
        {
            _userManager = userManager;
            _signManager = signManager;
            _config = cofig;
        }

        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Username);
                if (user != null)
                {
                    var id_token = await CreateToken(model);
                    return Ok(id_token);
                }
                return NotFound();
            }
            return BadRequest();
        }

        private async Task<object> CreateToken([FromBody] LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByEmailAsync(model.Username);
                if (user != null)
                {
                    // - false for lockOut on Failure
                    var result = await _signManager.CheckPasswordSignInAsync(user, model.Password, false);
                    if (result.Succeeded)
                    {
                        // Create the token
                        var claims = new[] {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };
                        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Tokens:key"]));
                        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                        var token = new JwtSecurityToken(
                            _config["Tokens:issuer"],
                            _config["Tokens:audience"],
                            claims,
                            expires: DateTime.UtcNow.AddMinutes(30),
                            signingCredentials: creds
                        );

                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo
                        };
                        return results;
                    }
                }
                return null;
            }
            return null;
        }
        [HttpPost]
        [Route("Decode")]
        public async Task<IActionResult> Decode(string token)
        {
            string secret = _config["Tokens:key"];
            try
            {
                var json = new JwtBuilder()
                    .WithSecret(secret)
                    .Decode(token);
                Console.WriteLine(json);
                return Ok(json);
            }
            catch (SecurityTokenExpiredException)
            {
                return BadRequest("Token has expired!");
            }
            catch (SignatureVerificationException)
            {
                Console.WriteLine("Token has invalid signature!");
            }
            return BadRequest();
        }
    }
}
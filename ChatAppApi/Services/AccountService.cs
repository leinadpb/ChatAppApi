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
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using ChatAppApi.Data;

namespace ChatAppApi.Services
{
    public class AccountService
    {
        private readonly AppDbContext _context;
        private readonly UserManager<UserApp> _userManager;
        private readonly SignInManager<UserApp> _signManager;
        private readonly IConfiguration _config;

        public AccountService(MessagesService srv,
          UserManager<UserApp> userManager, SignInManager<UserApp> signManager,
          IConfiguration cofig, AppDbContext ctx)
        {
            _userManager = userManager;
            _signManager = signManager;
            _config = cofig;
            _context = ctx;
        }

        public async Task<UserApp> AddUser(RegisterModel model)
        {
            if (model.Email != null && model.Password != null)
            {
                UserApp user = new UserApp {
                    Email = model.Email,
                    UserName = model.Email,
                    Firstname = model.Firstname,
                    Lastname = model.Lastname
                };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    return user;
                }
            }
            return null;
        }

        public async Task<object> CreateToken(LoginModel model)
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

    }
}

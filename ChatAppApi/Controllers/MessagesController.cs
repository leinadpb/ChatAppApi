using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatAppApi.Services;
using ChatAppApi.Models;
using Microsoft.AspNetCore.SignalR;
using ChatAppApi.Hubs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ChatAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class MessagesController : Controller
    {
        private readonly MessagesService _service;
        private readonly IHubContext<ChatHub> _chatHub;
        private readonly UserManager<UserApp> _userManager;
        private readonly SignInManager<UserApp> _signManager;

        private readonly IConfiguration _config;

        public MessagesController(MessagesService srv, IHubContext<ChatHub> hubContext,
            UserManager<UserApp> userManager, SignInManager<UserApp> signManager,
            IConfiguration cofig)
        {
            this._service = srv;
            _chatHub = hubContext;
            _userManager = userManager;
            _signManager = signManager;
            _config = cofig;
        }

        // GET: Messages
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            List<Message> msgs = await _service.GetAllAsync();
            if(msgs != null)
            {
                return Ok(msgs);
            }
            return NotFound();
        }

        //POST: Message
        [HttpPost]
        public async Task<IActionResult> Index([FromBody] Message message, string connId)
        {
            message.SentDate = DateTime.Now;
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            Message result = await _service.Add(message);
            if(result != null)
            {
                // call hub
                await this._chatHub.Clients.AllExcept(connId)
                    .SendAsync("ReceiveMessage", result);
                return Ok(message);
            }else
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Route("CreateToken")]
        [AllowAnonymous]
        public async Task<IActionResult> CreateToken([FromBody] LoginModel model)
        {
            Console.WriteLine($"HERE >>>>>>>>> Started!");
            if (ModelState.IsValid)
            {
                Console.WriteLine($"HERE >>>>>>>>> {model.Username}");
                Console.WriteLine($"HERE >>>>>>>>> {model.Password}");
                var user = await _userManager.FindByEmailAsync(model.Username);
                Console.WriteLine($"HERE >>>>>>>>> {user.Email}");
                Console.WriteLine($"HERE >>>>>>>>> {user.PasswordHash}");
                if (user != null)
                {
                    // - false for lockOut on Failure
                    var result = await _signManager.CheckPasswordSignInAsync(user, model.Password, false);
                    Console.WriteLine($"HERE >>>>>>>> PASSWORD MATCHED!!!!!: {result} ");
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

                        return Created("", results);
                    }
                }
                return NotFound();
            }
            return BadRequest();
        }
    }
}
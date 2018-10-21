using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ChatAppApi.Services;
using ChatAppApi.Models;
using Microsoft.AspNetCore.SignalR;
using ChatAppApi.Hubs;

namespace ChatAppApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : Controller
    {
        private readonly MessagesService _service;
        private readonly IHubContext<ChatHub> _chatHub;

        public MessagesController(MessagesService srv, IHubContext<ChatHub> hubContext)
        {
            this._service = srv;
            _chatHub = hubContext;
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
        public async Task<IActionResult> Index([FromBody] Message message)
        {
            Console.WriteLine($"HERE >>>>>>>>> {message}");
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }
            Message result = await _service.Add(message);
            if(result != null)
            {
                // call hub
                await this._chatHub.Clients.All.SendAsync("ReceiveMessage", result);
                return Ok(message);
            }else
            {
                return NotFound();
            }
        }
    }
}
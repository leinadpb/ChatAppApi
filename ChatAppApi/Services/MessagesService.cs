using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChatAppApi.Data;
using ChatAppApi.Models;
using ChatAppApi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppApi.Services
{
    public class MessagesService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ChatHub> _chatHub;

        public MessagesService(AppDbContext ctx, IHubContext<ChatHub> chatHub)
        {
            _context = ctx;
            _chatHub = chatHub;
        }

        public List<Message> GetAll()
        {
            return _context.Messages.Select(m => m).ToList();
        }

        public async Task<List<Message>> GetAllAsync()
        {
            return await Task.Run(() => {
                return _context.Messages.ToList();
            });
        }

        public async Task<Message> Add(Message msg)
        {
            await _context.AddAsync(msg);
            try
            {
               await _context.SaveChangesAsync();
               //await _chatHub.Clients.All.SendAsync("newMessageAdded");
               return msg;
            }catch(Exception exp)
            {
                Console.WriteLine($"Error {exp}");
            }
            return null;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ChatAppApi.Hubs
{
    public static class UserHandler
    {
        public static HashSet<string> ConnectedIds = new HashSet<string>();
    }

    public class ChatHub : Hub
    {
        public async Task SendMessage(string user, string msg)
        {
            await Clients.All.SendAsync("ReceiveMessage", user, msg);
        }

        public override Task OnConnectedAsync()
        {
            UserHandler.ConnectedIds.Add(Context.ConnectionId);
            int nums = UserHandler.ConnectedIds.Count();
            Console.WriteLine($"Nuevo cliente: {Context.ConnectionId} : Total conectados: {nums}");
            Clients.All.SendAsync("changeTotalMessages", nums);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            UserHandler.ConnectedIds.Remove(Context.ConnectionId);
            int nums = UserHandler.ConnectedIds.Count();
            Console.WriteLine($"Se ha ido un cliente: {Context.ConnectionId} : Total conectados: {nums}");
            Clients.All.SendAsync("changeTotalMessages", nums);
            return base.OnDisconnectedAsync(exception);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

namespace SignalRChat
{
    public class ChatHub : Hub
    {
        //methods that clients can call
        public async Task NewMessage(string message)
        {
            // Call the broadcastMessage method to update clients.
            await Clients.All.NewMessage(message);
        }

    }
}
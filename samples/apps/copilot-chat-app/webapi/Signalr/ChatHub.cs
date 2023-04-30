using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using SemanticKernel.Service.Skills;

namespace SemanticKernel.Service
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly IKernel _kernel;
        private readonly PromptSettings _promptSettings;

        public ChatHub(
            ILogger<ChatHub> logger,
            IDistributedCache cache,
            IConfiguration config,
            IKernel kernel,
            PromptSettings promptSettings
            )
        {
            this._logger = logger;
            this._cache = cache;
            this._config = config;
            this._kernel = kernel;
            this._promptSettings = promptSettings;
        }

#pragma warning disable IDE1006 // Naming Styles
        private async Task SendMessageToGroup(string groupName, string user, string message)

        {
            await this.Clients.Group(groupName).SendAsync("ReceiveMessage", user, message);
        }

        private async Task SendMessageToClient(string clientId, string user, string message)
        {
            await this.Clients.Client(clientId).SendAsync("ReceiveMessage", user, message);
        }

        public async Task SendMessage(string message, string user = "Bot")
        {
            // Get the name of the chat group for this user
            var groupName = await this._cache.GetStringAsync(this.Context.ConnectionId);
            await this.Clients.Group(groupName).SendAsync("StatusUpdate", "writing");
            // Send a response from the bot
            await Task.Delay(1000); // Simulate a delay
            
            await this.Clients.Group(groupName).SendAsync("StatusUpdate", "");
            //await SendMessageToClient(Context.ConnectionId, "Bot", _response);
        }

        public async Task JoinChat()
        {
            // Check if the user is already in a chat group
            var groupName = await this._cache.GetStringAsync(this.Context.ConnectionId);
            if (!string.IsNullOrEmpty(groupName))
            {
                await this.SendMessageToClient(this.Context.ConnectionId, "Bot", $"You are already in the {groupName} chat group!");
                return;
            }

            // Generate a unique ID for the chat group
            groupName = Guid.NewGuid().ToString();

            // Add the user to the chat group
            await this._cache.SetStringAsync(this.Context.ConnectionId, groupName);
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupName);

            // Send a welcome message to the user
            await this.SendMessageToClient(this.Context.ConnectionId, "Bot", $"Welcome to the {groupName} chat group!");
        }


        public async Task LeaveChat()
        {
            // Remove the user from their current chat group
            var groupName = await this._cache.GetStringAsync(this.Context.ConnectionId);
            if (string.IsNullOrEmpty(groupName))
            {
                await this.SendMessageToClient(this.Context.ConnectionId, "Bot", $"You are not currently in a chat group.");
                return;
            }

            await this._cache.RemoveAsync(this.Context.ConnectionId);
            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupName);

           
            await this.SendMessageToGroup(groupName, "Bot", $"{this.Context.ConnectionId} has left the chat group.");
            
        }
#pragma warning restore IDE1006 // Naming Styles
    }
}

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using Microsoft.Extensions.Options;
using SemanticKernel.Service.Skills;
using SemanticKernel.Service.Config;
using SemanticKernel.Service.Model;
using SemanticKernel.Service.Storage;
using Microsoft.SemanticKernel.Skills.OpenAPI.Skills;
using SemanticKernel.Service.Telecom;

namespace SemanticKernel.Service
{
    public class ChatHub : Hub
    {
        private readonly ILogger<ChatHub> _logger;
        private readonly IDistributedCache _cache;
        private readonly IConfiguration _config;
        private readonly IKernel _kernel;
        private readonly PromptSettings _promptSettings;
        private readonly ChatSessionRepository _chatSessionRepository;
        private readonly ChatMessageRepository _chatMessageRepository;
        private readonly IOptions<DocumentMemoryOptions> _documentMemoryOptions;
        private readonly CopilotChatPlanner _planner;
        private readonly IOptions<PlannerOptions> _plannerOptions;
        private readonly UserAccountService _userAccountService;

        public ChatHub(
            ILogger<ChatHub> logger,
            IDistributedCache cache,
            IConfiguration config,
            IKernel kernel,
            PromptSettings promptSettings,
            ChatSessionRepository chatSessionRepository,
            ChatMessageRepository chatMessageRepository,
            IOptions<DocumentMemoryOptions> documentMemoryOptions,
            CopilotChatPlanner planner,
            IOptions<PlannerOptions> plannerOptions,
            UserAccountService userAccountService
            )
        {
            this._logger = logger;
            this._cache = cache;
            this._config = config;
            this._kernel = kernel;
            this._promptSettings = promptSettings;
            this._chatSessionRepository = chatSessionRepository;
            this._chatMessageRepository = chatMessageRepository;
            this._documentMemoryOptions = documentMemoryOptions;
            this._planner = planner;
            this._plannerOptions = plannerOptions;
            this._userAccountService = userAccountService;
        }

        public enum StatusType
        {
            None = 0,
            Writing = 1,
            Searching = 2,
            Analyzing = 3
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

        public async Task SendStatusToGroup(string groupName, string message,StatusType statusType, string user="Bot")
        {
            await this.Clients.Group(groupName).SendAsync("StatusUpdate", statusType, message);
        }

        public async Task SendMessage(string user, string message)
        {
            // Get the name of the chat group for this user
            var groupName = await this._cache.GetStringAsync(this.Context.ConnectionId);
            await this.Clients.Group(groupName).SendAsync("StatusUpdate", StatusType.Analyzing,"Analizando pregunta...");
            // Send a response from the bot

            string skillName = "ChatSkill";
            string functionName = "Chat";

            SKContext result = await this.RunSkillAsync(skillName,functionName, user,message);

            await this.SendStatusToGroup(groupName, "", StatusType.None);
            await this.SendMessageToGroup(groupName, "Bot", result.Result);
        }

        private async Task<ContextVariables> createContextAsync(string input="")
        {
            var groupName = await this._cache.GetStringAsync(this.Context.ConnectionId);
            var contextVariables = new ContextVariables(input);
            var username = await this.GetUsernameAsync();
            var cuic = await this.GetCuicAsync();

            contextVariables.Set("chatId", groupName);
            contextVariables.Set("userId", this.Context.ConnectionId);
            contextVariables.Set("userName", username);
            contextVariables.Set("cuic", cuic);
            return contextVariables;
        }

        private async Task<SKContext> RunSkillAsync(string skillName, string functionName, string token="", string input="")
        {
            var contextVariables = await this.createContextAsync(input);

            if (!string.IsNullOrEmpty(token))
            {
                contextVariables["token"] = token;
            }
            

            // Run the function.
            SKContext result = await this.RunSkillAsync(skillName,functionName, contextVariables);
            return result;
        }

        private async Task<SKContext> RunSkillAsync(string skillName, string functionName, ContextVariables contextVariables)
        {
            
            this._kernel.RegisterNativeSkills(
                chatSessionRepository: this._chatSessionRepository,
                chatMessageRepository: this._chatMessageRepository,
                promptSettings: this._promptSettings,
                planner: this._planner,
                plannerOptions: this._plannerOptions.Value,
                documentMemoryOptions: this._documentMemoryOptions.Value,
                logger: this._logger,
                this);

            ISKFunction? function = null;

            function = this._kernel.Skills.GetFunction(skillName, functionName);

            // Run the function.
            SKContext result = await this._kernel.RunAsync(contextVariables, function);
            return result;
        }

        public async Task JoinChat(string token)
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
            await this.SetUserFromTokenAsync(token);

            SKContext result = await this.RunSkillAsync("ChatHistorySkill", "CreateChat");

            var initialBotMessage = JsonSerializer.Deserialize<Dictionary<string, object>>(result.Variables["initialBotMessage"].ToString());


            // Send a welcome message to the user
            await this.SendMessageToClient(this.Context.ConnectionId, "Bot", $"{initialBotMessage["content"]}");
        }

        private async Task SetUserFromTokenAsync(string token)
        {
            var userAccount = await this._userAccountService.GetUserAccountFromTokenAsync(token).ConfigureAwait(false);
            await this.SetUsernameAsync(userAccount.FirstName);
            await this.SetCuicAsync(userAccount.cuic);
        }

        public async Task SetUsernameAsync(string username)
        {
            await this._cache.SetStringAsync($"{this.Context.ConnectionId}_username", username).ConfigureAwait(false);
        }

        public async Task<string> GetUsernameAsync()
        {
            return await this._cache.GetStringAsync($"{this.Context.ConnectionId}_username").ConfigureAwait(false);
        }

        public async Task SetCuicAsync(string cuic)
        {
            await this._cache.SetStringAsync($"{this.Context.ConnectionId}_cuic", cuic).ConfigureAwait(false);
        }

        public async Task<string> GetCuicAsync()
        {
            return await this._cache.GetStringAsync($"{this.Context.ConnectionId}_cuic").ConfigureAwait(false);
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

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using SemanticKernel.Service.Storage;
using SemanticKernel.Service.Telecom;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SemanticKernel.Service.Skills
{
    public class TelecomContactSkill
    {
        private readonly IKernel _kernel;
        /// <summary>
        /// A repository to save and retrieve chat messages.
        /// </summary>
        private readonly ChatMessageRepository _chatMessageRepository;

        /// <summary>
        /// A repository to save and retrieve chat sessions.
        /// </summary>
        private readonly ChatSessionRepository _chatSessionRepository;

        private readonly PromptSettings _promptSettings;

        private const string Domain = "https://gestiononline.telecom.com.ar";
        private const string ContactService = "/v2/api/data/getClienteContactosByToken";
        private string _cuic = "01030500010912";
        private string _token = "70b1541c-d13f-4c0d-a91c-1c6ebda14d55";

        public TelecomContactSkill()
        {
            //TODO
        }

        public static async Task<string> DownloadContactDataAsync(string cuic, string token)
        {
            using var httpClient = new HttpClient();
            var uriBuilder = new UriBuilder(Domain);
            uriBuilder.Path = ContactService;
            uriBuilder.Query = $"cuic={cuic}&token={token}";

            var response = await httpClient.GetAsync(uriBuilder.Uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        [SKFunction("Devuelve la informacion de contactos de telecom disponibles para el usuario en formato json, usar esta informacion para contestar mensaje o pregunta sobre contactos, telefonos. No usar para resolver otros problemas que no sean exclusivamentes sobre personas de contacto. No mostrar la informacion en el formato recibido, usar formatos entendibles para un usuario en el contexto de un chat.")]
        public async Task<string> GetContactsAsync(string input, SKContext context)
        {
            string data = await DownloadContactDataAsync(this._cuic,this._token);
            string result = $@"Los contactos del usuario poseen la siguiente informacion en formato json, usar esta informacion para contestar cualquier mensaje o pregunta sobre contactos. No mostrar la informacion en el formato recibido, usar formatos entendibles para un usuario en el contexto de un chat.
            {data}";

            //var userId = context["userId"];
            //TelecomDataCollection.AddData(userId, "TELECOM_CONTACTOS", result);

            return result;
        }
    }
}

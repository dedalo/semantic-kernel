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
    public class TelecomFacturaSkill
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

        private const int Quantity = 12;
        private const string Domain = "https://gestiononline.telecom.com.ar";
        private const string SearchService = "/b2bgol-svc-500/api/search/searchObject";

        public TelecomFacturaSkill()
        {
            //TODO
        }

        [SKFunction("Descarga los datos desde el servicio rest")]
        public static async Task<string> DownloadDataAsync(string subscriptionId)
        {
            using var httpClient = new HttpClient();
            var uriBuilder = new UriBuilder(Domain);
            uriBuilder.Path = SearchService + "/3ScaleInvoices";
            uriBuilder.Query = $"subscriptionId={subscriptionId}&quantity={Quantity}";

            var response = await httpClient.GetAsync(uriBuilder.Uri);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        [SKFunction("Devuelve la informacion de facturas del cliente, incluyendo el monto o valor total y el saldo pendiente, tambien incluye la forma para amar un link de descarga y un link de pago. los montos estan expresados en ARS, y las fechas son GMT -3 usar un formato de fecha Latinoamericano al mostrar")]
        public async Task<string> GetFacturasAsync(string input, SKContext context)
        {
            input = "99776418";
            string data = await DownloadDataAsync(input);
            string result = $@"Las facturas del usuario poseen la siguiente informacion en formato json, usar esta informacion para contestar cualquier mensaje o pregunta sobre facturas o saldos, intentar responder preguntas abiertas con algo de informacion como una lista de facturas con fecha y monto, no pedir numeros de facturas o contratos u otros datos al cliente. el monto de la factura se indica en el campo adjustmentAmount y el saldo pendiente en balanceAmount.
            {data}";

            //var userId = context["userId"];
            //TelecomDataCollection.AddData(userId, "TELECOM_FACTURAS",result);

            return result;
        }
    }
}

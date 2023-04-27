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

        [SKFunction("Devuelve la informacion de facturas del usuario logueado, incluyendo el monto o valor total y el saldo pendiente, tambien incluye la forma para amar un link de descarga y un link de pago. los montos estan expresados en ARS, y las fechas son GMT -3 usar un formato de fecha Latinoamericano al mostrar")]
        [SKFunctionInput(Description = "El contrato para el que se necesitan las facturas")]
        [SKFunctionContextParameter(Name = "userId", Description = "Unique and persistent identifier for the user")]
        public async Task<string> GetFacturasAsync(string input, SKContext context)
        {
            input = "99776418";
            string data = await DownloadDataAsync(input);
            string result = $@"Las facturas del usuario poseen la siguiente informacion en formato json, usar esta informacion para contestar cualquier mensaje o pregunta sobre facturas o saldos, intentar responder preguntas abiertas con algo de informacion como la ultima factura, no pedir numeros de facturas o contratos al cliente sin dar opciones con informacion para elegir. no mostrar la informacion en el formato recibido, usar formatos entendibles para un usuario en el contexto de un chat. el monto de la factura se indica en el campo adjustmentAmount y el saldo pendiente en balanceAmount.
            {data}";

            var userId = context["userId"];
            TelecomDataCollection.AddData(userId, "TELECOM_FACTURAS",result);

            return result;
        }
    }
}

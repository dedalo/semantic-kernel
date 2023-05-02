using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using SemanticKernel.Service.Storage;
using SemanticKernel.Service.Telecom;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static SemanticKernel.Service.ChatHub;

namespace SemanticKernel.Service.Skills;

public class TelecomFacturaSkill
{
    private readonly ChatHub _chatHub;

    private const int Quantity = 12;
    private const string Domain = "https://gestiononline.telecom.com.ar";
    private const string SearchService = "/b2bgol-svc-500/api/search/searchObject";

    public TelecomFacturaSkill(ChatHub chatHub)
    {
        this._chatHub = chatHub;
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
    [SKFunctionContextParameter(Name = "token", Description = "telecom access token")]
    [SKFunctionContextParameter(Name = "cuic", Description = "cuic")]
    [SKFunctionContextParameter(Name = "chatId", Description = "chatid")]
    public async Task<string> GetFacturasAsync(string input, SKContext context)
    {
        var cuic = context["cuic"];
        var token = context["token"];
        var chatId = context["chatId"];
        await this._chatHub.SendStatusToGroup(chatId, "Buscando contratos...", StatusType.Searching);
        // bajo los contratos
        List<TelecomContract?> contracts = await TelecomContractSkill.GetContractsListAsync(cuic, token);

        await this._chatHub.SendStatusToGroup(chatId, "Buscando facturas...", StatusType.Searching);
        input = contracts[0].ID_SUSCRIPCION;
        string data = await DownloadDataAsync(input);
        string result = $@"Las facturas del usuario poseen la siguiente informacion en formato json, usar esta informacion para contestar cualquier mensaje o pregunta sobre facturas o saldos, Si la pregunta del usuario no es clara devolver la lista de facturas con fecha y monto, no pedir numeros de facturas o contratos u otros datos al cliente. el monto de la factura se indica en el campo adjustmentAmount y el saldo pendiente en balanceAmount.
            {data}";

        return result;
    }
}

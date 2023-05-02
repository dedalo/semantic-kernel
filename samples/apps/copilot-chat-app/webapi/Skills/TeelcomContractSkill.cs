using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using SemanticKernel.Service.Storage;
using SemanticKernel.Service.Telecom;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using static SemanticKernel.Service.ChatHub;
using System.Collections.Generic;

namespace SemanticKernel.Service.Skills;

public class TelecomContractSkill
{

    private readonly ChatHub _chatHub;

    private const string Domain = "https://gestiononline.telecom.com.ar";
    private const string ContactService = "/v2/api/search/fibercorp_contratosbytoken";

    public TelecomContractSkill(ChatHub chatHub)
    {
        this._chatHub = chatHub;
    }

    public static async Task<List<TelecomContract?>> GetContractsListAsync(string cuic, string token)
    {
        string json = await DownloadContractDataAsync(cuic, token);
        var response = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
        var rows = (JArray)response["rows"];

        List<TelecomContract?> contracts = rows.Select(row => row.ToObject<TelecomContract>()).ToList();

        return contracts;

    }

    public static async Task<string> DownloadContractDataAsync(string cuic, string token)
    {
        using var httpClient = new HttpClient();
        var uriBuilder = new UriBuilder(Domain);
        uriBuilder.Path = ContactService;
        uriBuilder.Query = $"cuic={cuic}&oauth_token={token}";

        var response = await httpClient.GetAsync(uriBuilder.Uri);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    [SKFunction("Contratos con su id y direccion, el id se llama idsuscripcion, usar esta informacion para contestar mensaje o pregunta sobre contratos y servicios que el cliente tiene contratados.")]
    [SKFunctionContextParameter(Name = "token", Description = "token")]
    [SKFunctionContextParameter(Name = "cuic", Description = "cuic")]
    public async Task<string> GetContractsAsync(SKContext context)
    {
        string token = context["token"];
        string cuic = context["cuic"];

        List<TelecomContract?> contracts = await GetContractsListAsync(cuic, token);

        string data = JsonConvert.SerializeObject(contracts.Take(10).ToList()); // limito a 10 contratos porque no puedo mandar tanto a openAI
        string result = $@"Los contratos o servicios que el usuario tiene con la empresa en formato json, usar esta informacion para contestar cualquier mensaje o pregunta sobre contratos o direcciones del cliente. No mostrar la informacion en el formato recibido, usar formatos entendibles para un usuario en el contexto de un chat.
            {data}";

        return result;
    }
}
public class TelecomContract
{
    public string ID_SUSCRIPCION { get; set; }
    public string DOMICILIO_COMPLETO_FACTURACION { get; set; }
}

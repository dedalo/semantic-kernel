using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;
using SemanticKernel.Service.Storage;
using SemanticKernel.Service.Telecom;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static SemanticKernel.Service.ChatHub;

namespace SemanticKernel.Service.Skills;

public class TelecomContactSkill
{
    private readonly ChatHub _chatHub;

    private const string Domain = "https://gestiononline.telecom.com.ar";
    private const string ContactService = "/v2/api/data/getClienteContactosByToken";

    public TelecomContactSkill(ChatHub chatHub)
    {
        this._chatHub = chatHub;
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

    [SKFunction("Devuelve la informacion de personas, ejecutivos de cuenta, y otro tipo de contactos, nombres, emails y telefonos disponibles para el usuario, usar esta informacion para contestar mensaje o pregunta sobre contactos, telefonos, si quiere comunicarse telefonicamente o si quiere hablar con una persona.")]
    [SKFunctionContextParameter(Name = "token", Description = "token")]
    [SKFunctionContextParameter(Name = "cuic", Description = "cuic")]
    [SKFunctionContextParameter(Name = "chatId", Description = "chatid")]
    public async Task<string> GetContactsAsync(SKContext context)
    {
        string token = context["token"];
        string cuic = context["cuic"];
        var chatId = context["chatId"];
        await this._chatHub.SendStatusToGroup(chatId, "Buscando contactos...", StatusType.Searching);
        string data = await DownloadContactDataAsync(cuic,token);
        string result = $@"Los contactos de la empresa disponibles para el usuario poseen la siguiente informacion en formato json, usar esta informacion para contestar cualquier mensaje o pregunta sobre contactos, telefonos, llamados. No mostrar la informacion en el formato recibido, usar formatos entendibles para un usuario en el contexto de un chat.
            {data}";

        return result;
    }
}

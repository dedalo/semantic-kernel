// Copyright (c) Microsoft. All rights reserved.

using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.SkillDefinition;
using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.CoreSkills;
using Microsoft.SemanticKernel.Memory;
using SemanticKernel.Service.Storage;

namespace SemanticKernel.Service.Skills;

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

    public TelecomFacturaSkill(
        IKernel kernel,
        ChatMessageRepository chatMessageRepository,
        ChatSessionRepository chatSessionRepository,
        PromptSettings promptSettings)
    {
        this._kernel = kernel;
        this._chatMessageRepository = chatMessageRepository;
        this._chatSessionRepository = chatSessionRepository;
        this._promptSettings = promptSettings;
    }

    [SKFunction("Devuelve la informacion de facturas del usuario logueado, incluyendo el monto total y el saldo pendiente, tambien incluye la forma para amar un link de descarga y un link de pago.")]
    public async Task<string> GetFacturasAsync(string input)
    {
        string result = "date,documentType,documentNumber,dueDate,amount,contract,letter,cotization\n2023-03-15T20:57:10.000-03:00,F,4264-01549076,2023-04-04T23:59:59.000-03:00,45254,99776418,A,\n2023-02-15T22:29:53.000-03:00,F,4264-01361544,2023-03-03T23:59:59.000-03:00,22627,99776418,A,\n2023-01-15T04:09:18.000-03:00,F,4264-01174550,2023-02-03T23:59:59.000-03:00,39930,99776418,A,\n2022-12-17T17:25:16.000-03:00,F,4264-00987880,2023-01-05T23:59:59.000-03:00,67881,99776418,A,\n2022-11-16T20:29:53.000-03:00,F,4264-00801363,2022-12-05T23:59:59.000-03:00,47916,99776418,A,\n2022-10-15T17:32:33.000-03:00,F,4264-00615211,2022-11-03T23:59:59.000-03:00,47916,99776418,A,\n2022-09-15T21:07:40.000-03:00,F,4264-00429123,2022-10-05T23:59:59.000-03:00,31097,99776418,A,\n2022-08-14T19:13:55.000-03:00,F,4264-00245079,2022-09-05T23:59:59.000-03:00,14278,99776418,A,\n2022-07-16T06:24:22.000-03:00,F,4264-00063736,2022-08-03T23:59:59.000-03:00,No saldo,99776418,A,\n2022-06-16T15:53:23.000-03:00,F,6723-04100890,2022-07-05T23:59:59.000-03:00,27285.5,99776418,A,\n2022-05-15T03:42:36.000-03:00,F,6723-03918946,2022-06-03T23:59:59.000-03:00,13007.5,99776418,A,\n2022-04-15T15:59:57.000-03:00,F,6723-03740154,2022-05-04T23:59:59.000-03:00,13007.5,99776418,A,";

        await this._kernel.Memory.SaveInformationAsync("telecom_facturas", result, "CUIT", "informacion de facturas");
        return result;
    }
}

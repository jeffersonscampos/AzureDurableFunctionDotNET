using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Logging;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace FunctionApp1
{
    #region Documentacao
    //https://learn.microsoft.com/pt-br/azure/azure-functions/functions-create-your-first-function-visual-studio
    //https://iarunpaul.github.io/jekyll/update/2023/07/20/durable-functions-in-short.html
    //https://learn.microsoft.com/pt-br/azure/azure-functions/functions-how-to-use-azure-function-app-settings?tabs=portal#development-limitations-in-the-azure-portal
    //https://learn.microsoft.com/pt-br/azure/azure-functions/functions-reference?tabs=blob&pivots=programming-language-csharp
    //https://learn.microsoft.com/pt-br/azure/azure-functions/functions-run-local?tabs=windows%2Cisolated-process%2Cnode-v4%2Cpython-v2%2Chttp-trigger%2Ccontainer-apps&pivots=programming-language-javascript
    //https://learn.microsoft.com/pt-br/azure/azure-functions/durable/durable-functions-isolated-create-first-csharp?pivots=code-editor-vscode
    //https://learn.microsoft.com/pt-br/azure/azure-functions/durable/quickstart-js-vscode?pivots=nodejs-model-v4
    //https://learn.microsoft.com/pt-br/azure/azure-functions/durable/durable-functions-configure-durable-functions-with-credentials
    //https://learn.microsoft.com/pt-br/azure/azure-functions/functions-bindings-service-bus?tabs=isolated-process%2Cextensionv5%2Cextensionv3&pivots=programming-language-csharp
    //https://learn.microsoft.com/pt-br/azure/azure-functions/functions-identity-access-azure-sql-with-managed-identity
    //https://learn.microsoft.com/pt-br/azure/azure-functions/functions-develop-vs-code?tabs=node-v3%2Cpython-v2%2Cisolated-process&pivots=programming-language-csharp#publish-to-azure
    //https://learn.microsoft.com/pt-br/azure/azure-functions/functions-how-to-github-actions?tabs=windows%2Cdotnet&pivots=method-manual
    //https://learn.microsoft.com/pt-br/azure/azure-functions/functions-manually-run-non-http?tabs=azure-portal
    //https://learn.microsoft.com/pt-br/azure/azure-functions/manage-connections?tabs=csharp
    //https://learn.microsoft.com/pt-br/azure/azure-functions/add-bindings-existing-function?tabs=python-v2%2Cisolated-process%2Cnode-v3&pivots=programming-language-csharp

    // CLOUD Shell - comando para obter a URL da Function:
    // func azure functionapp list-functions "FuncaoAprovarPedidoNova3" --show-keys

    #endregion //Fim_Documentacao

    public class PedidosAprovacaoDTO : ISerializable
    {
        public int IdPedido { get; set; }
        public Decimal Valor { get; set; }

        public EnuPedidosEtapaAprovacao Etapa { get; set; }

        public bool ResultadoProcessamento { get; set; }

        public PedidosAprovacaoDTO(int idPedido, decimal valorPedido, EnuPedidosEtapaAprovacao etapaAprovacao)
        {
            IdPedido = idPedido;
            Valor = valorPedido;
            Etapa = etapaAprovacao;
        }

        public PedidosAprovacaoDTO()
        {
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("IdPedido", IdPedido);
            info.AddValue("Valor", Valor);
            info.AddValue("Etapa", Etapa.ToString());
        }
    }

    public enum EnuPedidosEtapaAprovacao 
    {
        [Description("PedidoCriado")]
        PedidoCriado = 1,
        [Description("PedidoEmFilaDeAnalise")]
        PedidoEmFilaDeAnalise = 2,
        [Description("PedidoAtribuidoParaAnalise")]
        PedidoAtribuidoParaAnalie = 3,
        [Description("PedidoEmAnalise")]
        PedidoEmAnalise = 4,
        [Description("PedidoEmValidacaoDePreco")]
        PedidoEmValidacaoDePreco = 5,
        [Description("AprovacaoFinalizada")]
        AprovacaoFinalizada = 6

    }


    public static class PedidosAprovacaoFunc
    {
        private const string nomeFuncaoOrquestradora = "PedidosAprovacaoFuncOrquestradora";
        private const Decimal limiteAprovacaoPedidos = 500000.00M;

        [Function(nomeFuncaoOrquestradora)]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] TaskOrchestrationContext context)            
        {
            ILogger logger = context.CreateReplaySafeLogger("PedidosAprovacaoFuncOrquestradora");            
            var outputs = new List<string>();
            

            var parametros = context.GetInput<PedidosAprovacaoDTO>();
            var idPedido = parametros?.IdPedido;
            var valor = parametros?.Valor;         

            var dados = parametros;

            // ProcessarEtapaAprovacao --> PedidoCriado
            var resultadoPedidoCriado = await context.CallActivityAsync<bool>(nameof(ProcessarEtapaAprovacao), dados);
            outputs.Add("PedidoCriado " + resultadoPedidoCriado.ToString());

            logger.LogInformation($"Aprovação de Pedido... id {idPedido} valor {valor} etapa {dados.Etapa} resultado {resultadoPedidoCriado}");

            // ProcessarEtapaAprovacao --> PedidoEmFilaDeAnalise
            var resultadoPedidoEmFilaDeAnalise = false;
            if (resultadoPedidoCriado)
            {             
                dados.Etapa = EnuPedidosEtapaAprovacao.PedidoEmFilaDeAnalise;


                resultadoPedidoEmFilaDeAnalise = await context.CallActivityAsync<bool>(nameof(ProcessarEtapaAprovacao), dados);
                outputs.Add("PedidoEmFilaDeAnalise " + resultadoPedidoEmFilaDeAnalise.ToString());

                logger.LogInformation($"Aprovação de Pedido... id {idPedido} valor {valor} etapa {dados.Etapa} resultado {resultadoPedidoEmFilaDeAnalise}");
            }

            // ProcessarEtapaAprovacao --> PedidoAtribuidoParaAnalie
            var resultadoPedidoAtribuidoParaAnalise = false;
            if (resultadoPedidoEmFilaDeAnalise) 
            {
                dados.Etapa = EnuPedidosEtapaAprovacao.PedidoAtribuidoParaAnalie;

                resultadoPedidoAtribuidoParaAnalise = await context.CallActivityAsync<bool>(nameof(ProcessarEtapaAprovacao), dados);
                outputs.Add("PedidoAtribuidoParaAnalie " + resultadoPedidoAtribuidoParaAnalise.ToString());

                logger.LogInformation($"Aprovação de Pedido... id {idPedido} valor {valor} etapa {dados.Etapa} resultado {resultadoPedidoAtribuidoParaAnalise}");
            }

            // ProcessarEtapaAprovacao --> PedidoEmAnalise
            var resultadoPedidoEmAnalise = false;
            if (resultadoPedidoAtribuidoParaAnalise) {                
                dados.Etapa = EnuPedidosEtapaAprovacao.PedidoEmAnalise;

                resultadoPedidoEmAnalise = await context.CallActivityAsync<bool>(nameof(ProcessarEtapaAprovacao), dados);
                outputs.Add("PedidoEmAnalise " + resultadoPedidoEmAnalise.ToString());

                logger.LogInformation($"Aprovação de Pedido... id {idPedido} valor {valor} etapa {dados.Etapa} resultado {resultadoPedidoEmAnalise}");
            }

            // ProcessarEtapaAprovacao --> PedidoEmValidacaoDePreco
            var resultadoPedidoEmValidacaoDePreco = false;
            if (resultadoPedidoEmAnalise)
            {
                dados.Etapa = EnuPedidosEtapaAprovacao.PedidoEmValidacaoDePreco;

                resultadoPedidoEmValidacaoDePreco = await context.CallActivityAsync<bool>(nameof(ProcessarEtapaAprovacao), dados);
                outputs.Add("PedidoEmValidacaoDePreco " + resultadoPedidoEmValidacaoDePreco.ToString());

                logger.LogInformation($"Aprovação de Pedido... id {idPedido} valor {valor} etapa {dados.Etapa} resultado {resultadoPedidoEmValidacaoDePreco}");
            }

            // ProcessarEtapaAprovacao --> AprovacaoFinalizada
            var resultadoAprovacaoFinalizada = false;
            if (resultadoPedidoEmValidacaoDePreco)
            {
                dados.Etapa = EnuPedidosEtapaAprovacao.AprovacaoFinalizada;

                resultadoAprovacaoFinalizada = await context.CallActivityAsync<bool>(nameof(ProcessarEtapaAprovacao), dados);
                outputs.Add("AprovacaoFinalizada " + resultadoAprovacaoFinalizada.ToString());

                logger.LogInformation($"Aprovação de Pedido... id {idPedido} valor {valor} etapa {dados.Etapa} resultado {resultadoAprovacaoFinalizada}");
            }            
            
            return outputs;
        }

        [Function(nameof(ProcessarEtapaAprovacao))]        
        public static bool ProcessarEtapaAprovacao([ActivityTrigger] PedidosAprovacaoDTO dados, FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger("ProcessarEtapaAprovacao");

            //--: var idPedido = Convert.ToInt32(dados.Split('|')[0]);
            //--: var valor = Convert.ToDecimal(dados.Split('|')[1]);
            //--: var etapa = dados.Split('|')[2];

            var idPedido = dados.IdPedido;
            var valor = dados.Valor;
            var etapa = dados.Etapa.ToString();

            logger.LogInformation($"Processando Aprovação do Pedido {idPedido}, valor {valor}, etapa {etapa}.");
            //--> if (etapa != "PedidoEmValidacaoDePreco")

            if (dados.Etapa != EnuPedidosEtapaAprovacao.PedidoEmValidacaoDePreco)
                return true;
            else
                return valor <= limiteAprovacaoPedidos;            
        }

        [Function(nameof(HttpStart))]
        public static async Task<HttpResponseData> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req,
            [DurableClient] DurableTaskClient client,
            FunctionContext executionContext)
        {
            ILogger logger = executionContext.GetLogger(nameof(HttpStart));

            var parametros = new PedidosAprovacaoDTO();
            parametros.IdPedido = Convert.ToInt32(req.Query["idPedido"]);
            parametros.Valor = Convert.ToDecimal(req.Query["valor"]);
            parametros.Etapa = EnuPedidosEtapaAprovacao.PedidoCriado;

        // Function input comes from the request content.
        string instanceId = await client.ScheduleNewOrchestrationInstanceAsync(nomeFuncaoOrquestradora, parametros);

        logger.LogInformation("Started orchestration with ID = '{instanceId}'.", instanceId);

            // Returns an HTTP 202 response with an instance management payload.
            // See https://learn.microsoft.com/azure/azure-functions/durable/durable-functions-http-api#start-orchestration
            HttpResponseData resposta = client.CreateCheckStatusResponse(req, instanceId);
            return resposta;
        }
    }
}

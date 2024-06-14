using Azure.AI.OpenAI;
using Azure;
using OpenAI.FrontEnd.Models;
using Microsoft.Extensions.Options;

namespace OpenAI.FrontEnd.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly OpenAIServiceSettings _openAIServiceSettings;
        
        public OpenAIService(IOptions<OpenAIServiceSettings> openAIServiceSettings)
        {
            _openAIServiceSettings = openAIServiceSettings.Value;
        }

        public async Task<Models.Response> CallAzureOpenAI(string prompt, string instruction = null)
        {
            var client = new OpenAIClient(new Uri(_openAIServiceSettings.ApiBase), new AzureKeyCredential(_openAIServiceSettings.ApiKey!));

            var interactionHistory = new List<Interaction>();

            if (!string.IsNullOrWhiteSpace(instruction))
            {
                interactionHistory.Add(new Interaction { Role = "user", Content = instruction });
            }

            prompt = "The logged user is James Bond. He is a member of the compliance team. " + prompt;

            interactionHistory.Add(new Interaction { Role = "user", Content = prompt });

            var chatMessages = new List<ChatRequestUserMessage>();
            foreach (var interaction in interactionHistory)
            {
                chatMessages.Add(new ChatRequestUserMessage(interaction.Content)
                {
                    Role = interaction.Role,
                });
            }

            var chatCompletionsOptions = new ChatCompletionsOptions()
            {
                AzureExtensionsOptions = new AzureChatExtensionsOptions()
                {
                    Extensions =
                    {
                        new AzureCognitiveSearchChatExtensionConfiguration()
                        {
                            SearchEndpoint = new Uri(_openAIServiceSettings.SearchEndpoint),
                            IndexName = _openAIServiceSettings.SearchIndexName,
                            Key = _openAIServiceSettings.SearchKey,
                            QueryType = AzureCognitiveSearchQueryType.Simple,
                        },
                    },
                },
                DeploymentName = _openAIServiceSettings.DeploymentName,
                MaxTokens = 800,
                Temperature = 0,
            };

            foreach (var interaction in interactionHistory)
            {
                chatCompletionsOptions.Messages.Add(new ChatRequestUserMessage(interaction.Content)
                {
                    Role = interaction.Role,
                });
            }

            var response = await client.GetChatCompletionsAsync(chatCompletionsOptions);

            var message = response.Value.Choices[0].Message;

            var answer = new Models.Response();

            answer.Content = message.Content;
            answer.Citations = message.AzureExtensionsContext.Messages[0].Content;

            if (answer.Content == "The requested information is not available in the retrieved data. Please try another query or topic.")
                answer.Citations = null;

            return answer;
        }
    }
}

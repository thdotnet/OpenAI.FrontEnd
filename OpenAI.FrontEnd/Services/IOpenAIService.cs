using OpenAI.FrontEnd.Models;

namespace OpenAI.FrontEnd.Services
{
    public interface IOpenAIService
    {
        Task<Response> CallAzureOpenAI(string prompt, string instruction = null);
    }
}
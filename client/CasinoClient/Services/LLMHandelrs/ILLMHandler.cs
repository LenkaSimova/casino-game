
using System.Threading.Tasks;

namespace CasinoClient.Services.LLMHandlers;

interface ILLMHandler
{
    void AddSystemMessage(string systemMessage);
    Task<string> SendMessageAsync(string userMessage);

    public void ClearHistory();

}
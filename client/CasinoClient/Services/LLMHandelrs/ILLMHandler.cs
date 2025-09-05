
using System.Threading.Tasks;

namespace CasinoClient.Services.LLMHandlers;

public interface ILLMHandler
{
    void AddSystemMessage(string systemMessage);
    Task<string> SendMessageAsync(string userMessage);

    public void ClearHistory();

}
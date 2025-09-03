

using System.Threading.Tasks;
using Refit;
using System;




namespace CasinoClient.Services.LLMHandlers
{
    public class GeminiLLMHandler : ILLMHandler
    {
        private readonly IGeminiApi _geminiApi;

        private readonly string _apikey = Environment.GetEnvironmentVariable("GEMINI_API_KEY") ?? throw new InvalidOperationException("GEMINI_API_KEY environment variable is not set");
        private readonly string _model;


        public GeminiLLMHandler(string baseUrl = "https://generativelanguage.googleapis.com", string model = "gemini-2.0-flash")
        {
            _geminiApi = RestService.For<IGeminiApi>(baseUrl);
            _model = model;
        }

        /// <summary>
        /// Adds a system message to the conversation context
        /// </summary>
        /// <param name="systemMessage">The system message to add</param>
        public void AddSystemMessage(string systemMessage)
        {
            // to be implemented
        }

        /// <summary>
        /// Sends a user message and gets a streaming response from the LLM
        /// </summary>
        /// <param name="userMessage">The user's message</param>
        /// <returns>The complete response from the LLM</returns>
        public async Task<string> SendMessageAsync(string userMessage)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                return string.Empty;

            var request = new ContentRequest
            {
                contents = new[]
                {
                    new Content
                    {
                        parts = new[]
                        {
                            new Part { text = userMessage }
                        }
                    }
                }
            };
            // Get response from LLM
            var response = await _geminiApi.GenerateContentAsync(_model, request, _apikey);

            var responseText = response.Content.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "";

            return responseText;
        }



        /// <summary>
        /// Clears the chat history
        /// </summary>
        public void ClearHistory()
        {
            // to be implemented
        }
    }
}
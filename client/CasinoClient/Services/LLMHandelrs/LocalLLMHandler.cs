using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using OllamaSharp;

namespace CasinoClient.Services.LLMHandlers
{
    public class LocalLLMHandler : ILLMHandler
    {
        private readonly IChatClient _chatClient;
        private readonly List<ChatMessage> _chatHistory;

        public LocalLLMHandler(string baseUrl = "http://localhost:11434/", string model = "gemma3:270m")
        {
            _chatClient = new OllamaApiClient(new Uri(baseUrl), model);
            _chatHistory = new List<ChatMessage>();
        }

        /// <summary>
        /// Adds a system message to the conversation context
        /// </summary>
        /// <param name="systemMessage">The system message to add</param>
        public void AddSystemMessage(string systemMessage)
        {
            _chatHistory.Add(new ChatMessage(ChatRole.System, systemMessage));
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

            // Add user message to chat history
            _chatHistory.Add(new ChatMessage(ChatRole.User, userMessage));

            // Get streaming response from LLM
            var response = "";
            await foreach (ChatResponseUpdate item in _chatClient.GetStreamingResponseAsync(_chatHistory))
            {
                response += item.Text;
            }

            // Add assistant response to chat history
            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));

            return response;
        }

        /// <summary>
        /// Sends a user message and gets a streaming response with a callback for each chunk
        /// </summary>
        /// <param name="userMessage">The user's message</param>
        /// <param name="onChunkReceived">Callback called for each chunk of the response</param>
        /// <returns>The complete response from the LLM</returns>
        public async Task<string> SendMessageStreamAsync(string userMessage, Action<string>? onChunkReceived = null)
        {
            if (string.IsNullOrWhiteSpace(userMessage))
                throw new ArgumentException("User message cannot be null or empty", nameof(userMessage));

            // Add user message to chat history
            _chatHistory.Add(new ChatMessage(ChatRole.User, userMessage));

            // Get streaming response from LLM
            var response = "";
            await foreach (ChatResponseUpdate item in _chatClient.GetStreamingResponseAsync(_chatHistory))
            {
                response += item.Text;
                onChunkReceived?.Invoke(item.Text);
            }

            // Add assistant response to chat history
            _chatHistory.Add(new ChatMessage(ChatRole.Assistant, response));

            return response;
        }


        /// <summary>
        /// Clears the chat history
        /// </summary>
        public void ClearHistory()
        {
            _chatHistory.Clear();
        }
    }
}
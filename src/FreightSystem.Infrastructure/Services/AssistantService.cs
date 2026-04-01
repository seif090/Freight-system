using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Settings;
using Microsoft.Extensions.Options;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;

namespace FreightSystem.Infrastructure.Services
{
    public class AssistantService : IAssistantService
    {
        private readonly List<string> _history = new();
        private readonly HttpClient _httpClient;
        private readonly LlmSettings _llmSettings;

        public AssistantService(HttpClient httpClient, IOptions<LlmSettings> llmOptions)
        {
            _httpClient = httpClient;
            _llmSettings = llmOptions.Value;
        }

        public async Task<AssistantResponse> ExecuteAsync(AssistantRequest request)
        {
            // Simple context window + rule engine
            if (!string.IsNullOrEmpty(request.ContextHash))
            {
                _history.Add(request.ContextHash);
                if (_history.Count > 20) _history.RemoveAt(0);
            }

            var input = request.Input.Trim();
            var contextText = string.Join(" ", _history);
            var prompt = $"You are a freight operations AI assistant. Context: {contextText}. User: {input}";
            var llmResult = await QueryLlmAsync(prompt);

            var response = new AssistantResponse { Success = true, Results = llmResult.text, NextAction = llmResult.nextAction, Command = llmResult.command, TokenUsage = llmResult.tokenUsage };

            // rule engine fallback when command cannot be extracted
            if (string.IsNullOrEmpty(response.Command))
            {
                var lower = input.ToLowerInvariant();
                if (lower.Contains("reroute")) response.Command = "reroute";
                else if (lower.Contains("gantt")) response.Command = "establish_gantt";
                else if (lower.Contains("crew")) response.Command = "crew_dispatch";
                else response.Command = "chat";
            }

            if (!string.IsNullOrEmpty(request.ContextHash))
            {
                _history.Add(request.ContextHash);
                if (_history.Count > 20) _history.RemoveAt(0);
            }

            return response;
        }

        private async Task<(string text, string command, string nextAction, int tokenUsage)> QueryLlmAsync(string prompt)
        {
            if (_llmSettings.Provider.ToLowerInvariant() == "anthropic" && !string.IsNullOrWhiteSpace(_llmSettings.ClaudeApiKey))
            {
                var body = new
                {
                    model = _llmSettings.ClaudeModel,
                    prompt = prompt,
                    max_tokens_to_sample = _llmSettings.MaxTokens,
                    stop_sequences = new[] {"\nHuman:"}
                };
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _llmSettings.ClaudeApiKey);
                var res = await _httpClient.PostAsJsonAsync("https://api.anthropic.com/v1/complete", body);
                res.EnsureSuccessStatusCode();
                var obj = await res.Content.ReadFromJsonAsync<dynamic>();
                var text = (string)obj?.completion ?? "";
                var command = ExtractCommand(text);
                var nextAction = ExtractNextAction(text);
                return (text, command, nextAction, text.Length / 4);
            }
            else if (!string.IsNullOrWhiteSpace(_llmSettings.OpenAiApiKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _llmSettings.OpenAiApiKey);
                var body = new
                {
                    model = _llmSettings.OpenAiModel,
                    messages = new[] {
                        new { role = "system", content = "You are a freight operations assistant. Optimize and route commands." },
                        new { role = "user", content = prompt }
                    },
                    max_tokens = _llmSettings.MaxTokens,
                    temperature = 0.2
                };
                var res = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", body);
                res.EnsureSuccessStatusCode();
                var obj = await res.Content.ReadFromJsonAsync<dynamic>();
                var text = (string)obj?.choices[0]?.message?.content ?? "";
                int tokenUsage = obj?.usage?.total_tokens ?? (text.Length / 4);
                var command = ExtractCommand(text);
                var nextAction = ExtractNextAction(text);
                return (text, command, nextAction, tokenUsage);
            }

            return ("LLM keys not configured. Skipping.", "chat", "Provide command with reroute/gantt/crew dispatch.", 0);
        }

        private static string ExtractCommand(string text)
        {
            var t = text.ToLowerInvariant();
            if (t.Contains("reroute")) return "reroute";
            if (t.Contains("gantt")) return "establish_gantt";
            if (t.Contains("crew dispatch") || t.Contains("crew_dispatch")) return "crew_dispatch";
            return string.Empty;
        }

        private static string ExtractNextAction(string text)
        {
            if (text.ToLowerInvariant().Contains("reroute")) return "/advancedoperations/shipments/{id}/optimize-route";
            if (text.ToLowerInvariant().Contains("gantt")) return "/analytics/operations-cockpit";
            if (text.ToLowerInvariant().Contains("crew")) return "/advancedoperations/dispatch-actions";
            return "";
        }

        public Task<AssistantResponse> ProcessWebhookCommandAsync(string webhookType, object payload)
        {
            var response = new AssistantResponse { Success = true };
            response.Command = webhookType;
            response.Results = "Received webhook command: " + webhookType;

            switch (webhookType.ToLowerInvariant())
            {
                case "reroute":
                    response.NextAction = "optimization queue post-processed";
                    break;
                case "establish_gantt":
                    response.NextAction = "schedule milestones updated";
                    break;
                case "crew_dispatch":
                    response.NextAction = "crew alerted";
                    break;
                default:
                    response.NextAction = "unknown webhook";
                    response.Success = false;
                    break;
            }

            return Task.FromResult(response);
        }
    }
}

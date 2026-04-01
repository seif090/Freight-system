using FreightSystem.Application.Interfaces;
using FreightSystem.Core.Settings;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Net.Http.Json;

namespace FreightSystem.Infrastructure.Services
{
    public class AssistantService : IAssistantService
    {
        private readonly List<string> _history = new();
        private readonly HttpClient _httpClient;
        private readonly LlmSettings _llmSettings;
        private readonly IEventBus _eventBus;
        private readonly ILogger<AssistantService> _logger;

        public AssistantService(HttpClient httpClient, IOptions<LlmSettings> llmOptions, IEventBus eventBus, ILogger<AssistantService> logger)
        {
            _httpClient = httpClient;
            _llmSettings = llmOptions.Value;
            _eventBus = eventBus;
            _logger = logger;
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

            var response = new AssistantResponse
            {
                Success = llmResult.Success,
                Results = llmResult.Text,
                Command = llmResult.Command,
                NextAction = llmResult.NextAction,
                TokenUsage = llmResult.TokenUsage,
                Provider = llmResult.Provider,
                EstimatedCostUsd = llmResult.EstimatedCostUsd
            };

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

            // Analytics event for token usage + cost
            try
            {
                await _eventBus.PublishAsync("llm-assistant-usage", new
                {
                    request.UserId,
                    request.Input,
                    response.Provider,
                    response.Command,
                    response.TokenUsage,
                    response.EstimatedCostUsd,
                    response.Success,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to publish llm-assistant-usage event.");
            }

            return response;
        }

        private record LlmResult(bool Success, string Text, string Command, string NextAction, int TokenUsage, double EstimatedCostUsd, string Provider, string ErrorMessage);

        private async Task<LlmResult> QueryLlmAsync(string prompt)
        {
            var requestedProvider = string.IsNullOrWhiteSpace(_llmSettings.Provider) ? "openai" : _llmSettings.Provider.Trim().ToLowerInvariant();
            LlmResult result;

            if (requestedProvider == "anthropic" || requestedProvider == "claude")
            {
                result = await InvokeClaudeAsync(prompt);
                if (result.Success) return result;

                _logger.LogWarning("Primary provider Claude/Anthropic failed. Attempting OpenAI fallback: {Error}", result.ErrorMessage);

                result = await InvokeOpenAiAsync(prompt);
                if (result.Success) return result;
            }
            else // openai preferred (default) or anything else
            {
                result = await InvokeOpenAiAsync(prompt);
                if (result.Success) return result;

                _logger.LogWarning("Primary provider OpenAI failed. Attempting Claude fallback: {Error}", result.ErrorMessage);

                result = await InvokeClaudeAsync(prompt);
                if (result.Success) return result;
            }

            return new LlmResult(false, "LLM keys not configured or all providers failed.", "chat", "Provide command with reroute/gantt/crew dispatch.", 0, 0.0, requestedProvider, result.ErrorMessage);
        }

        private async Task<LlmResult> InvokeOpenAiAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(_llmSettings.OpenAiApiKey))
            {
                return new LlmResult(false, "OpenAI API key is not configured.", string.Empty, string.Empty, 0, 0.0, "openai", "Missing OpenAI key");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _llmSettings.OpenAiApiKey);

            var body = new
            {
                model = _llmSettings.OpenAiModel,
                messages = new[]
                {
                    new { role = "system", content = "You are a freight operations assistant. Optimize and route commands." },
                    new { role = "user", content = prompt }
                },
                max_tokens = _llmSettings.MaxTokens,
                temperature = 0.2
            };

            try
            {
                var res = await _httpClient.PostAsJsonAsync("https://api.openai.com/v1/chat/completions", body);
                res.EnsureSuccessStatusCode();

                var obj = await res.Content.ReadFromJsonAsync<dynamic>();
                var text = (string)obj?.choices?[0]?.message?.content ?? string.Empty;
                int tokens = obj?.usage?.total_tokens ?? (text.Length / 4);
                var command = ExtractCommand(text);
                var nextAction = ExtractNextAction(text);
                var cost = Math.Round(tokens * 0.000002, 6, MidpointRounding.AwayFromZero);

                return new LlmResult(true, text, command, nextAction, tokens, cost, "openai", string.Empty);
            }
            catch (Exception ex)
            {
                return new LlmResult(false, string.Empty, string.Empty, string.Empty, 0, 0.0, "openai", ex.Message);
            }
        }

        private async Task<LlmResult> InvokeClaudeAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(_llmSettings.ClaudeApiKey))
            {
                return new LlmResult(false, "Claude API key is not configured.", string.Empty, string.Empty, 0, 0.0, "claude", "Missing Claude key");
            }

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _llmSettings.ClaudeApiKey);

            var body = new
            {
                model = _llmSettings.ClaudeModel,
                prompt = prompt,
                max_tokens_to_sample = _llmSettings.MaxTokens,
                stop_sequences = new[] { "\nHuman:" }
            };

            try
            {
                var res = await _httpClient.PostAsJsonAsync("https://api.anthropic.com/v1/complete", body);
                res.EnsureSuccessStatusCode();
                var obj = await res.Content.ReadFromJsonAsync<dynamic>();
                var text = (string)obj?.completion ?? string.Empty;
                int tokens = text.Length / 4;
                var command = ExtractCommand(text);
                var nextAction = ExtractNextAction(text);
                var cost = Math.Round(tokens * 0.0000018, 6, MidpointRounding.AwayFromZero);

                return new LlmResult(true, text, command, nextAction, tokens, cost, "claude", string.Empty);
            }
            catch (Exception ex)
            {
                return new LlmResult(false, string.Empty, string.Empty, string.Empty, 0, 0.0, "claude", ex.Message);
            }
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
            return string.Empty;
        }

        public Task<AssistantResponse> ProcessWebhookCommandAsync(string webhookType, object payload)
        {
            var response = new AssistantResponse { Success = true };
            response.Provider = "webhook";
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

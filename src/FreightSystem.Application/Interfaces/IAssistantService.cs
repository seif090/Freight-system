namespace FreightSystem.Application.Interfaces
{
    public class AssistantRequest
    {
        public string UserId { get; set; } = "anonymous";
        public string Input { get; set; } = string.Empty;
        public string ContextHash { get; set; } = string.Empty;
    }

    public class AssistantResponse
    {
        public string Command { get; set; } = string.Empty;
        public string Results { get; set; } = string.Empty;
        public string NextAction { get; set; } = string.Empty;
        public bool Success { get; set; }
        public int TokenUsage { get; set; }
    }

    public interface IAssistantService
    {
        Task<AssistantResponse> ExecuteAsync(AssistantRequest request);
        Task<AssistantResponse> ProcessWebhookCommandAsync(string webhookType, object payload);
    }
}
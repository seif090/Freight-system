namespace FreightSystem.Core.Settings
{
    public class LlmSettings
    {
        public string Provider { get; set; } = "openai";
        public string OpenAiApiKey { get; set; } = string.Empty;
        public string OpenAiModel { get; set; } = "gpt-4.1";
        public string ClaudeApiKey { get; set; } = string.Empty;
        public string ClaudeModel { get; set; } = "claude-3.5-mini";
        public int MaxTokens { get; set; } = 1500;
        public Dictionary<string, double> ModelCostPerToken { get; set; } = new()
        {
            ["gpt-4.1"] = 0.000002,
            ["gpt-3.5-turbo"] = 0.000001,
            ["claude-3.5-mini"] = 0.0000018,
            ["claude-3.5-pro"] = 0.0000022
        };
        public int CircuitBreakerFailureThreshold { get; set; } = 5;
        public TimeSpan CircuitBreakerWindow { get; set; } = TimeSpan.FromSeconds(60);
        public TimeSpan CircuitBreakerResetDuration { get; set; } = TimeSpan.FromMinutes(2);
    }
}
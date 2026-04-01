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
    }
}
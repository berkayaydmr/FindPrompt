namespace LearnPrompt.Infrastructure.Options
{
    public class OpenAIOptions
    {
        public const string SectionName = "OpenAI";

        public string Endpoint { get; set; } = "https://api.openai.com/v1";

        public string Model { get; set; } = "gpt-4o-mini";

        /// <summary>
        /// Optional override for chat completion model. Falls back to <see cref="Model"/> when empty.
        /// </summary>
        public string? ChatModel { get; set; }

        /// <summary>
        /// API key should be supplied via user-secrets or environment variables in production.
        /// </summary>
        public string ApiKey { get; set; } = string.Empty;

        public double Temperature { get; set; } = 0.2;

        /// <summary>
        /// Optional override for chat completion temperature. Falls back to <see cref="Temperature"/>.
        /// </summary>
        public double ChatTemperature { get; set; } = 0.3;
    }
}


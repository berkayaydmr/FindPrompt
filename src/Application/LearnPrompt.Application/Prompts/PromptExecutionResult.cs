namespace LearnPrompt.Application.Prompts
{
    public record PromptExecutionResult(
        PromptResult Prompt,
        string Response);
}


using LearnPrompt.Domain.Entities;

namespace LearnPrompt.Application.Prompts
{
    public record PromptRequest(
        Course Course,
        CourseTopic Topic,
        string? UserName,
        string? UserLevel,
        int TopK = 6);
}


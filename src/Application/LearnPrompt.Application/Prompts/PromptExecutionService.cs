using System;
using System.Threading;
using System.Threading.Tasks;

namespace LearnPrompt.Application.Prompts
{
    public class PromptExecutionService : IPromptExecutionService
    {
        private readonly IPromptService _promptService;
        private readonly IOpenAIChatService _chatService;

        public PromptExecutionService(
            IPromptService promptService,
            IOpenAIChatService chatService)
        {
            _promptService = promptService;
            _chatService = chatService;
        }

        public async Task<PromptExecutionResult> ExecuteAsync(
            int topicId,
            string ownerId,
            string? userName,
            string? userLevel,
            int topK = 6,
            CancellationToken cancellationToken = default)
        {
            var promptResult = await _promptService.BuildPromptForTopicAsync(
                topicId,
                ownerId,
                userName,
                userLevel,
                topK,
                cancellationToken);

            var response = await _chatService.GenerateCompletionAsync(
                promptResult.Prompt,
                cancellationToken);

            return new PromptExecutionResult(promptResult, response);
        }
    }
}


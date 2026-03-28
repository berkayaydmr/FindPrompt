using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Application.Repositories;
using LearnPrompt.Application.Topics;

namespace LearnPrompt.Application.Prompts
{
    public class PromptService : IPromptService
    {
        private readonly ICourseRepository _courseRepository;
        private readonly IPromptBuilder _promptBuilder;

        public PromptService(
            ICourseRepository courseRepository,
            IPromptBuilder promptBuilder)
        {
            _courseRepository = courseRepository;
            _promptBuilder = promptBuilder;
        }

        public async Task<PromptResult> BuildPromptForTopicAsync(
            int topicId,
            string ownerId,
            string? userName,
            string? userLevel,
            int topK = 6,
            CancellationToken cancellationToken = default)
        {
            var topic = await _courseRepository.GetTopicByIdAsync(topicId)
                ?? throw new KeyNotFoundException("Topic not found.");

            if (!string.Equals(topic.Course.OwnerId, ownerId, StringComparison.Ordinal))
            {
                throw new UnauthorizedAccessException();
            }

            var request = new PromptRequest(topic.Course, topic, userName, userLevel, topK);
            return await _promptBuilder.BuildPromptAsync(request, cancellationToken);
        }
    }
}


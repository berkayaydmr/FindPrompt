using System.Collections.Generic;

namespace LearnPrompt.Application.Processing
{
    public interface IChunkingService
    {
        IEnumerable<string> SplitIntoChunks(string text);
    }
}


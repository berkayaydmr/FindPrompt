using System;
using System.Collections.Generic;
using System.Text;
using LearnPrompt.Application.Processing;

namespace LearnPrompt.Infrastructure.Processing
{
    public class FixedWindowChunkingService : IChunkingService
    {
        private readonly int _chunkSize;
        private readonly int _overlap;

        public FixedWindowChunkingService(int chunkSize = 800, int overlap = 120)
        {
            if (chunkSize <= 0) throw new ArgumentOutOfRangeException(nameof(chunkSize));
            if (overlap < 0 || overlap >= chunkSize) throw new ArgumentOutOfRangeException(nameof(overlap));

            _chunkSize = chunkSize;
            _overlap = overlap;
        }

        public IEnumerable<string> SplitIntoChunks(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            var normalized = text.ReplaceLineEndings("\n");
            var buffer = normalized.Trim();
            var position = 0;

            while (position < buffer.Length)
            {
                var length = Math.Min(_chunkSize, buffer.Length - position);
                var chunk = buffer.Substring(position, length).Trim();

                if (!string.IsNullOrWhiteSpace(chunk))
                {
                    yield return chunk;
                }

                if (position + length >= buffer.Length)
                {
                    break;
                }

                position += _chunkSize - _overlap;
            }
        }
    }
}


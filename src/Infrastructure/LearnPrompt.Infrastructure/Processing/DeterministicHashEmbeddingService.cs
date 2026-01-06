using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Application.Processing;

namespace LearnPrompt.Infrastructure.Processing
{
    public class DeterministicHashEmbeddingService : IEmbeddingService
    {
        private const int VectorDimension = 96;

        public Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var normalized = text ?? string.Empty;
            var bytes = Encoding.UTF8.GetBytes(normalized);

            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(bytes);

            var repeats = (int)Math.Ceiling((double)(VectorDimension * sizeof(float)) / hash.Length);
            var buffer = Enumerable.Repeat(hash, repeats)
                .SelectMany(b => b)
                .Take(VectorDimension * sizeof(float))
                .ToArray();

            var result = new float[VectorDimension];
            for (var i = 0; i < VectorDimension; i++)
            {
                var offset = i * sizeof(float);
                var value = BitConverter.ToUInt32(buffer, offset);
                result[i] = (value / (float)uint.MaxValue) * 2f - 1f;
            }

            return Task.FromResult(result);
        }
    }
}


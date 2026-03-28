using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using LearnPrompt.Application.Processing;
using LearnPrompt.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace LearnPrompt.Infrastructure.Processing
{
    public class QdrantVectorStore : IVectorStore
    {
        private readonly QdrantClient _client;
        private readonly QdrantOptions _options;
        private readonly IEmbeddingService _embeddingService;

        public QdrantVectorStore(
            IOptions<QdrantOptions> options,
            IEmbeddingService embeddingService)
        {
            _options = options.Value;
            _embeddingService = embeddingService;
            
            // Parse endpoint to extract host, port, and protocol
            var endpoint = _options.Endpoint;
            var useHttps = endpoint.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
            
            // Remove protocol
            var hostAndPort = endpoint.Replace("https://", "").Replace("http://", "");
            
            // Extract host and port
            string host;
            int port;
            
            if (hostAndPort.Contains(':'))
            {
                var parts = hostAndPort.Split(':');
                host = parts[0];
                port = int.Parse(parts[1]);
            }
            else
            {
                host = hostAndPort;
                port = useHttps ? 6334 : 6333; // Default gRPC or HTTP port
            }
            
            _client = new QdrantClient(
                host: host,
                port: port,
                https: useHttps,
                apiKey: _options.ApiKey);
        }

        public async Task UpsertChunkEmbeddingAsync(
            VectorStoreRecord record,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Ensure collection exists
            await EnsureCollectionExistsAsync(cancellationToken);

            var pointId = new PointId { Uuid = record.ChunkId.ToString() };
            
            var payload = new Dictionary<string, Value>
            {
                ["courseId"] = record.CourseId,
                ["courseFileId"] = record.CourseFileId,
                ["rawText"] = record.RawText,
                ["chunkId"] = record.ChunkId.ToString()
            };

            var pointStruct = new PointStruct
            {
                Id = pointId,
                Vectors = record.Embedding.ToArray(),
                Payload = { payload }
            };

            await _client.UpsertAsync(
                collectionName: _options.CollectionName,
                points: new[] { pointStruct },
                cancellationToken: cancellationToken);
        }

        public async Task<IReadOnlyList<VectorSearchResult>> SearchAsync(
            int courseId,
            string query,
            int topK,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Generate query embedding
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(query, cancellationToken);

            // Ensure collection exists
            await EnsureCollectionExistsAsync(cancellationToken);

            // Create filter for courseId
            var filter = new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "courseId",
                            Match = new Match { Integer = courseId }
                        }
                    }
                }
            };

            // Search in Qdrant
            var searchResults = await _client.SearchAsync(
                collectionName: _options.CollectionName,
                vector: queryEmbedding.ToArray(),
                filter: filter,
                limit: (ulong)topK,
                cancellationToken: cancellationToken);

            // Map results to VectorSearchResult
            var results = searchResults.Select(result => new VectorSearchResult
            {
                ChunkId = Guid.Parse(result.Payload["chunkId"].StringValue),
                CourseId = (int)result.Payload["courseId"].IntegerValue,
                CourseFileId = (int)result.Payload["courseFileId"].IntegerValue,
                RawText = result.Payload["rawText"].StringValue,
                Score = result.Score
            }).ToList();

            return results;
        }

        public async Task RemoveByCourseFileAsync(
            int courseFileId,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();

            // Create filter for courseFileId
            var filter = new Filter
            {
                Must =
                {
                    new Condition
                    {
                        Field = new FieldCondition
                        {
                            Key = "courseFileId",
                            Match = new Match { Integer = courseFileId }
                        }
                    }
                }
            };

            // Delete points matching the filter
            await _client.DeleteAsync(
                collectionName: _options.CollectionName,
                filter: filter,
                cancellationToken: cancellationToken);
        }

        private async Task EnsureCollectionExistsAsync(CancellationToken cancellationToken)
        {
            try
            {
                // Try to get collection info
                await _client.GetCollectionInfoAsync(_options.CollectionName, cancellationToken);
                // Collection exists, assume it's correctly configured
            }
            catch
            {
                // Collection doesn't exist or error occurred, create it
                await CreateCollectionAsync(cancellationToken);
            }
        }

        private async Task CreateCollectionAsync(CancellationToken cancellationToken)
        {
            // Delete collection if it exists (to recreate with correct dimensions)
            try
            {
                await _client.DeleteCollectionAsync(_options.CollectionName, cancellationToken: cancellationToken);
            }
            catch
            {
                // Collection doesn't exist, ignore
            }

            await _client.CreateCollectionAsync(
                collectionName: _options.CollectionName,
                vectorsConfig: new VectorParams
                {
                    Size = (ulong)_options.VectorSize,
                    Distance = Distance.Cosine
                },
                cancellationToken: cancellationToken);

            // Create payload index for courseId
            await _client.CreatePayloadIndexAsync(
                collectionName: _options.CollectionName,
                fieldName: "courseId",
                schemaType: PayloadSchemaType.Integer,
                cancellationToken: cancellationToken);

            // Create payload index for courseFileId
            await _client.CreatePayloadIndexAsync(
                collectionName: _options.CollectionName,
                fieldName: "courseFileId",
                schemaType: PayloadSchemaType.Integer,
                cancellationToken: cancellationToken);
        }
    }
}


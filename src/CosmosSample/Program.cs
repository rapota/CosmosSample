using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CosmosSample
{
    internal class Program
    {
        private const int SourceSystemCount = 10;
        private const int TargetSystemsCount = 10;

        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            CosmosClientOptions options = new()
            {
                HttpClientFactory = () => new HttpClient(new HttpClientHandler()
                {
                    ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
                }),
                ConnectionMode = ConnectionMode.Gateway,
                LimitToEndpoint = true
            };

            using CosmosClient client = new(
                accountEndpoint: "https://localhost:8081/",
                authKeyOrResourceToken: "C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw=="
                // disable TLS/SSL validation
                , clientOptions: options
            );

            // By id
            await using (Benchmark benchmark = new Benchmark(client, SourceSystemCount, TargetSystemsCount, useIdAsPartitionKey: true))
            {
                await benchmark.CreateContainerAsync(false);
                //await benchmark.CreateContainerAsync(true);
                //await benchmark.SeedDataAsync(1000);

                //await benchmark.QueryAllDataAsync();

                await benchmark.QueryBySourceTargetDataAsync("SourceSystem-1", "TargetSystem-1");

                //string id = Guid.Empty.ToString();
                //ItemResponse<TranslationRule> readResponse = await benchmark.Container!.ReadItemAsync<TranslationRule>(id, new PartitionKey("/SourceSystem"));
                //ItemResponse<JObject> result = await benchmark.Container!.ReadItemAsync<JObject>(id, PartitionKey.None);
            }
            
            // By system
            await using (Benchmark benchmark = new Benchmark(client, SourceSystemCount, TargetSystemsCount, useIdAsPartitionKey: false))
            {
                await benchmark.CreateContainerAsync(false);
                //await benchmark.CreateContainerAsync(true);
                //await benchmark.SeedDataAsync(1000);

                //await benchmark.QueryAllDataAsync();

                await benchmark.QueryBySourceTargetDataAsync("SourceSystem-1", "TargetSystem-1");

                //string id = Guid.Empty.ToString();
                //ItemResponse<TranslationRule> readResponse = await benchmark.Container!.ReadItemAsync<TranslationRule>(id, new PartitionKey("/SourceSystem"));
                //ItemResponse<JObject> result = await benchmark.Container!.ReadItemAsync<JObject>(id, PartitionKey.None);
            }

            Console.WriteLine("Done.");
        }
    }
}
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using System.ComponentModel;

namespace CosmosSample
{
    internal class Program
    {
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

            await using (Benchmark benchmark = new Benchmark(client, sourceSystemCount: 10, targetSystemsCount: 10))
            {
                //await benchmark.ResetContainerAsync();
                //await benchmark.SeedDataAsync(10);

                await benchmark.CreateContainerAsync();

                await benchmark.QueryAllDataAsync();

                string id = Guid.Empty.ToString();
                //ItemResponse<TranslationRule> readResponse = await benchmark.Container!.ReadItemAsync<TranslationRule>(id, new PartitionKey("/SourceSystem"));
                //ItemResponse<JObject> result = await benchmark.Container!.ReadItemAsync<JObject>(id, PartitionKey.None);
            }

            Console.WriteLine("Done.");
        }
    }
}
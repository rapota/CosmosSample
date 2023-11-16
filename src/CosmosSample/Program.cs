using Microsoft.Azure.Cosmos;

namespace CosmosSample
{
    internal class Program
    {
        private const int SourceSystemCount = 100;
        private const int TargetSystemsCount = 100;

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
            Console.WriteLine("By id.");
            await using (Benchmark benchmark = new Benchmark(client, SourceSystemCount, TargetSystemsCount, useIdAsPartitionKey: true))
            {
                await benchmark.CreateContainerAsync(false);
                //await benchmark.CreateContainerAsync(true);
                //await benchmark.SeedDataAsync(10000);

                //await benchmark.QueryAllDataAsync();

                await benchmark.QueryBySourceTargetDataAsync("SourceSystem-1", "TargetSystem-1", usePartitionKey: false);
                await benchmark.LinqQueryBySourceTargetDataAsync("SourceSystem-1", "TargetSystem-1");

                //string id = Guid.Empty.ToString();
                //ItemResponse<TranslationRule> readResponse = await benchmark.Container!.ReadItemAsync<TranslationRule>(id, PartitionKey.Null);
                //Console.WriteLine($"Found item: {readResponse.Resource} {readResponse.RequestCharge} RUs");
            }

            // By key
            Console.WriteLine();
            Console.WriteLine("By key.");
            await using (Benchmark benchmark = new Benchmark(client, SourceSystemCount, TargetSystemsCount, useIdAsPartitionKey: false))
            {
                await benchmark.CreateContainerAsync(false);
                //await benchmark.CreateContainerAsync(true);
                //await benchmark.SeedDataAsync(10000);

                //await benchmark.QueryAllDataAsync();

                await benchmark.QueryBySourceTargetDataAsync("SourceSystem-1", "TargetSystem-1", usePartitionKey: true);
                await benchmark.LinqQueryBySourceTargetDataAsync("SourceSystem-1", "TargetSystem-1");

                Console.WriteLine();
                string id = Guid.Empty.ToString();
                id = "2049b695-653b-4fad-ac99-97350113c119";
                ItemResponse<TranslationRule> readResponse = await benchmark.Container!.ReadItemAsync<TranslationRule>(id, new PartitionKey("SourceSystem-1" + "TargetSystem-1"));
                Console.WriteLine($"Found item: {readResponse.Resource} {readResponse.RequestCharge} RUs");
            }

            Console.WriteLine("Done.");
        }
    }
}
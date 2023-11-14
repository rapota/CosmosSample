using System.Diagnostics;
using Microsoft.Azure.Cosmos;

namespace CosmosSample;

public sealed class Benchmark : IAsyncDisposable
{
    private const string DatabaseName = "index-experiments";
    private const string ContainerName = "transtations";

    private readonly CosmosClient _client;
    private readonly TranslationRuleFactory _translationRuleFactory;

    public Container? Container { get; private set; }

    public Benchmark(CosmosClient client, int sourceSystemCount, int targetSystemsCount)
    {
        _client = client;
        _translationRuleFactory = new TranslationRuleFactory(sourceSystemCount, targetSystemsCount);
    }

    public async ValueTask DisposeAsync()
    {
        if (Container != null)
        {
            //await Container.DeleteContainerAsync();
        }
    }

    public async Task CreateContainerAsync()
    {
        DatabaseResponse databaseResponse = await _client.CreateDatabaseIfNotExistsAsync(
            id: DatabaseName,
            throughput: 400);
        Database database = databaseResponse.Database;

        ContainerResponse containerResponse = await database.CreateContainerIfNotExistsAsync(
            id: ContainerName,
            partitionKeyPath: "/SourceSystem"
            //partitionKeyPath: "/id"
        );
        Container = containerResponse.Container;
    }

    public async Task ResetContainerAsync()
    {
        DatabaseResponse databaseResponse = await _client.CreateDatabaseIfNotExistsAsync(
            id: DatabaseName,
            throughput: 400);
        Database database = databaseResponse.Database;

        try
        {
            Container container0 = database.GetContainer(ContainerName);
            await container0.DeleteContainerAsync();
            Console.WriteLine("Removed old container.");
        }
        catch (CosmosException)
        {
        }

        await CreateContainerAsync();
    }

    public async Task SeedDataAsync(int count)
    {
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < count; i++)
        {
            TranslationRule translationRule = _translationRuleFactory.Create();
            if (i == 0)
            {
                translationRule.id = Guid.Empty;
            }

            ItemResponse<TranslationRule> itemResponse = await Container!.CreateItemAsync(translationRule);
            //await Container!.CreateItemAsync(
            //    translationRule,
            //    partitionKey: new PartitionKey("/SourceSystem"));
            Console.WriteLine("Create item {0} {1} RUs", translationRule.id, itemResponse.RequestCharge);
        }

        sw.Stop();
        Console.WriteLine("Seeded {0} items in {1}.", count, sw.Elapsed);
    }

    public async Task QueryAllDataAsync()
    {
        using FeedIterator<TranslationRule> feed = Container!.GetItemQueryIterator<TranslationRule>(queryText: $"SELECT * FROM {ContainerName}");
        while (feed.HasMoreResults)
        {
            FeedResponse<TranslationRule> response = await feed.ReadNextAsync();
            foreach (TranslationRule? item in response)
            {
                Console.WriteLine($"Found item:\t{item.id}\t{response.RequestCharge} RUs");
            }
        }
    }
}
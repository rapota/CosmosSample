using System.Diagnostics;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace CosmosSample;

public sealed class Benchmark : IAsyncDisposable
{
    private const string DatabaseName = "index-experiments";

    private readonly CosmosClient _client;
    private readonly TranslationRuleFactory _translationRuleFactory;
    private readonly string _containerName;
    private readonly string _partitionKeyPath;

    public Container? Container { get; private set; }

    public Benchmark(CosmosClient client, int sourceSystemCount, int targetSystemsCount, bool useIdAsPartitionKey)
    {
        _client = client;
        _translationRuleFactory = new TranslationRuleFactory(sourceSystemCount, targetSystemsCount);

        _partitionKeyPath = useIdAsPartitionKey
            ? "/id"
            : "/PartitionKey";

        _containerName = useIdAsPartitionKey
            ? "transtationid"
            : "transtationsystem";
    }

    public async ValueTask DisposeAsync()
    {
        if (Container != null)
        {
            //await Container.DeleteContainerAsync();
        }
    }

    public async Task CreateContainerAsync(bool deleteExisting)
    {
        DatabaseResponse databaseResponse = await _client.CreateDatabaseIfNotExistsAsync(
            id: DatabaseName,
            throughput: 400);
        Database database = databaseResponse.Database;

        if (deleteExisting)
        {
            try
            {
                Container container0 = database.GetContainer(_containerName);
                await container0.DeleteContainerAsync();
                Console.WriteLine("Removed old container.");
            }
            catch (CosmosException)
            {
            }
        }

        ContainerResponse containerResponse = await database.CreateContainerIfNotExistsAsync(
            id: _containerName,
            partitionKeyPath: _partitionKeyPath
        );
        Container = containerResponse.Container;
    }

    public async Task SeedDataAsync(int count)
    {
        var sw = Stopwatch.StartNew();
        for (int i = 0; i < count; i++)
        {
            TranslationRule translationRule = _translationRuleFactory.Create();
            //if (i == 0)
            //{
            //    translationRule.id = Guid.Empty;
            //}

            ItemResponse<TranslationRule> itemResponse = await Container!.CreateItemAsync(translationRule);
            //await Container!.CreateItemAsync(
            //    translationRule,
            //    partitionKey: new PartitionKey("/SourceSystem"));

            Console.WriteLine("Create item #{0} {1} {2} RUs", i, translationRule.id, itemResponse.RequestCharge);
        }

        sw.Stop();
        Console.WriteLine("Seeded {0} items in {1}.", count, sw.Elapsed);
    }

    public async Task SeedDataAsync2(int count)
    {
        List<TranslationRule> translationRules = Enumerable.Range(1, count)
            .Select(
                i =>
                {
                    TranslationRule translationRule = _translationRuleFactory.Create();
                    if (i == 0)
                    {
                        translationRule.id = Guid.Empty;
                    }

                    return translationRule;
                })
            .ToList();

        var sw = Stopwatch.StartNew();

        await Parallel.ForEachAsync(
            translationRules,
            async (x, ct) =>
            {
                ItemResponse<TranslationRule> itemResponse = await Container!.CreateItemAsync(x);
                Console.WriteLine("Create item {0} {1} RUs", x.id, itemResponse.RequestCharge);
            });

        sw.Stop();

        Console.WriteLine("Seeded {0} items in {1}.", count, sw.Elapsed);
    }

    public async Task QueryAllDataAsync()
    {
        Console.WriteLine("Query all.");
        using FeedIterator<TranslationRule> feed = Container!.GetItemQueryIterator<TranslationRule>(queryText: $"SELECT * FROM {_containerName}");
        while (feed.HasMoreResults)
        {
            FeedResponse<TranslationRule> response = await feed.ReadNextAsync();
            foreach (TranslationRule? item in response)
            {
                Console.WriteLine($"Found item: {item.id} {item} {response.RequestCharge} RUs");
                break;
            }
        }
    }

    public async Task QueryBySourceTargetDataAsync(string sourceSystem, string targetSystem, bool usePartitionKey)
    {
        Console.WriteLine("Get by source and target.");

        QueryDefinition parameterizedQuery = new QueryDefinition(
                query: $"SELECT * FROM {_containerName} p WHERE p.SourceSystem = @ss AND p.TargetSystem = @ts"
            )
            .WithParameter("@ss", sourceSystem)
            .WithParameter("@ts", targetSystem);

        QueryRequestOptions? qo = usePartitionKey
            ? new QueryRequestOptions
            {
                PartitionKey = new PartitionKey(sourceSystem + targetSystem)
            }
            : null;

        using FeedIterator<TranslationRule> feed = Container!.GetItemQueryIterator<TranslationRule>(
            queryDefinition: parameterizedQuery,
            requestOptions: qo);
        while (feed.HasMoreResults)
        {
            FeedResponse<TranslationRule> response = await feed.ReadNextAsync();
            foreach (TranslationRule? item in response)
            {
                Console.WriteLine($"Found item: {item.id} {item} {response.RequestCharge} RUs");
                //break;
            }
        }
    }

    public async Task LinqQueryBySourceTargetDataAsync(string sourceSystem, string targetSystem)
    {
        Console.WriteLine("Ling by source and target.");

        IOrderedQueryable<TranslationRule> linqQueryable = Container!.GetItemLinqQueryable<TranslationRule>();

        using FeedIterator<TranslationRule> feed = linqQueryable
            .Where(x => x.SourceSystem == sourceSystem && x.TargetSystem == targetSystem)
            .ToFeedIterator();

        while (feed.HasMoreResults)
        {
            FeedResponse<TranslationRule> response = await feed.ReadNextAsync();
            foreach (TranslationRule? item in response)
            {
                Console.WriteLine($"Found item: {item.id} {item} {response.RequestCharge} RUs");
                //break;
            }
        }
    }
}
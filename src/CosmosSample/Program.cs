using Microsoft.Azure.Cosmos;

namespace CosmosSample
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            CosmosClientOptions options = new ()
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

            Database database = await client.CreateDatabaseIfNotExistsAsync(
                id: "cosmicworks",
                throughput: 400
            );

            Container container = await database.CreateContainerIfNotExistsAsync(
                id: "products",
                partitionKeyPath: "/id"
            );

            var item = new Product("68719518371", "Kiama classic surfboard", 123);

            await container.UpsertItemAsync(item);

            var query = new QueryDefinition(
                    query: "SELECT * FROM products p WHERE p.category = @category"
                )
                .WithParameter("@category", "gear-surf-surfboards");
        }
    }

    public record Product(
        string id,
        string name,
        int quantity
    );
}
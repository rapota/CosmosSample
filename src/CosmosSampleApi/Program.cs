using CosmosSampleApi;
using Microsoft.Azure.Cosmos;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<TranslationContext>(opt =>
{
    opt.UseCosmos(
        connectionString: "AccountEndpoint=https://localhost:8081/;AccountKey=C2y6yDjf5/R+ob0N8A7Cgv30VRDJIWEHLM+4QDU5DE2nQ9nDuVTqobD4b8mGGyPMbIZnqyMsEcaGQy67XIw/Jw==",
        databaseName: "index-experiments",
        cb =>
        {
            cb.HttpClientFactory(() => new HttpClient(new HttpClientHandler()
            {
                ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
            }, disposeHandler: false));
            cb.ConnectionMode(ConnectionMode.Gateway);
            cb.LimitToEndpoint(true);
        });
});


builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.MapControllers();

app.Run();

using CosmosSample;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CosmosSampleApi;

public class TranslationContext : DbContext
{
    public TranslationContext(DbContextOptions<TranslationContext> options) : base(options) { }

    public DbSet<TranslationRule> TranslationRules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer(TranslationRuleEntityTypeConfiguration.ContainerName);
        modelBuilder.ApplyConfiguration(new TranslationRuleEntityTypeConfiguration());
    }

    public class TranslationRuleEntityTypeConfiguration : IEntityTypeConfiguration<TranslationRule>
    {
        public const string ContainerName = "transtationsystem";

        public void Configure(EntityTypeBuilder<TranslationRule> builder)
        {
            builder.ToContainer(ContainerName);

            builder.HasPartitionKey(c => c.PartitionKey);

            //builder.HasNoDiscriminator();
            //builder.Property(p => p.Id).ToJsonProperty("id");
        }
    }
}

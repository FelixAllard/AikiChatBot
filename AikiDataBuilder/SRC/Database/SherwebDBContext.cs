using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Sherweb.Apis.ServiceProvider.Models;
using JsonSerializer = Newtonsoft.Json.JsonSerializer;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;
using System.Collections.Generic;

namespace AikiDataBuilder.Database;

public class SherwebDBContext : DbContext
{
    public SherwebDBContext(DbContextOptions<SherwebDBContext> options)
        : base(options)
    {
    }

    public DbSet<SherwebModel> Customers { get; set; }
    public DbSet<PlatformUsage> PlatformUsages { get; set; }
    public DbSet<MeterUsage> MeterUsages { get; set; }
    public DbSet<PlatformDetails> PlatformDetails { get; set; }
    public DbSet<ReceivableCharges> ReceivableCharges { get; set; }
    public DbSet<ReceivableCharge> ReceivableCharge { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<SubscriptionFees> SubscriptionFees { get; set; }
    public DbSet<CommitmentTerm> CommitmentTerms { get; set; }
    public DbSet<RenewalConfiguration> RenewalConfigurations { get; set; }
    public DbSet<CommittedMinimalQuantity> CommittedMinimalQuantities { get; set; }
    public DbSet<Platform> Platforms { get; set; }
    public DbSet<LocalizedName> LocalizedNames { get; set; }
    public DbSet<PayableCharges> PayableCharges { get; set; }
    public DbSet<PayableCharge> PayableCharge { get; set; }
    public DbSet<Deduction> Deductions { get; set; }
    public DbSet<Fee> Fees { get; set; }
    public DbSet<Invoice> Invoices { get; set; }
    public DbSet<Tax> Taxes { get; set; }
    public DbSet<Tag> Tags { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Create value converters
        var listStringConverter = new ValueConverter<List<string>, string>(
            v => System.Text.Json.JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
            v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null) ?? new List<string>()
        );

        var dictionaryConverter = new ValueConverter<Dictionary<string, object>, string>(
            v => System.Text.Json.JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
            v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (JsonSerializerOptions)null) ?? new Dictionary<string, object>()
        );

        // Apply converters
        modelBuilder.Entity<Customer>()
            .Property(c => c.Path)
            .HasConversion(listStringConverter);

        modelBuilder.Entity<PlatformDetails>()
            .Property(p => p.Details)
            .HasConversion(dictionaryConverter);

        // Add indexes
        modelBuilder.Entity<Customer>().HasIndex(c => c.DisplayName);
        modelBuilder.Entity<PayableCharge>().HasIndex(pc => pc.ChargeId);
        modelBuilder.Entity<Invoice>().HasIndex(i => i.Number);
    }
}
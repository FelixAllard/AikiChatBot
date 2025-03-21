using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using AikiDataBuilder.Model.Sherweb.Database;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace AikiDataBuilder.Database;

/// <summary>
/// This class is the DB context. I made all the relations so it should bne easy to navigate through it.
/// Keep in mind that migrations are done through the EF tools,
/// and a migration is already provided in the Migrations folder
/// </summary>
public class SherwebDbContext : DbContext
{
    public SherwebDbContext(DbContextOptions<SherwebDbContext> options) : base(options) { }

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

    /// <summary>
    /// I will create all the relations here
    /// </summary>
    /// <param name="modelBuilder">Provided by the EF tool</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure JSON conversions
        var listStringConverter = new ValueConverter<List<string>, string>(
            v => JsonSerializer.Serialize(v ?? new List<string>(), (JsonSerializerOptions)null),
            v => JsonSerializer.Deserialize<List<string>>(v ?? "[]", (JsonSerializerOptions)null) ?? new List<string>()
        );

        modelBuilder.Entity<SherwebModel>()
            .Property(c => c.Path)
            .HasConversion(listStringConverter);

        modelBuilder.Entity<PlatformDetails>()
            .Property(p => p.Details)
            .HasColumnType("nvarchar(max)");

        // Configure relationships
        modelBuilder.Entity<SherwebModel>(entity =>
        {
            entity.HasMany(c => c.Platform)
                .WithOne()
                .HasForeignKey("CustomerId")
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(c => c.ReceivableCharges)
                .WithOne(r => r.Customer)
                .HasForeignKey<ReceivableCharges>(r => r.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(c => c.Subscriptions)
                .WithOne(s => s.Customer)
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PlatformUsage>(entity =>
        {
            entity.HasMany(p => p.MeterUsages)
                .WithOne(m => m.PlatformUsage)
                .HasForeignKey(m => m.PlatformUsageId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(p => p.PlatformDetails)
                .WithOne(pd => pd.PlatformUsage)
                .HasForeignKey<PlatformDetails>(pd => pd.PlatformUsageId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ReceivableCharges>(entity =>
        {
            entity.HasMany(rc => rc.Charges)
                .WithOne(c => c.ReceivableCharges)
                .HasForeignKey(c => c.ReceivableChargesId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Subscription>(entity =>
        {
            entity.HasOne(s => s.Fees)
                .WithOne(f => f.Subscription)
                .HasForeignKey<SubscriptionFees>(f => f.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(s => s.CommitmentTerm)
                .WithOne(ct => ct.Subscription)
                .HasForeignKey<CommitmentTerm>(ct => ct.SubscriptionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommitmentTerm>(entity =>
        {
            entity.HasOne(ct => ct.RenewalConfiguration)
                .WithOne(rc => rc.CommitmentTerm)
                .HasForeignKey<RenewalConfiguration>(rc => rc.CommitmentTermId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(ct => ct.CommittedMinimalQuantities)
                .WithOne(cmq => cmq.CommitmentTerm)
                .HasForeignKey(cmq => cmq.CommitmentTermId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Platform>(entity =>
        {
            entity.HasMany(p => p.Name)
                .WithOne(ln => ln.Platform)
                .HasForeignKey(ln => ln.PlatformId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PayableCharges>(entity =>
        {
            entity.HasMany(pc => pc.Charges)
                .WithOne(c => c.PayableCharges)
                .HasForeignKey(c => c.PayableChargesId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PayableCharge>(entity =>
        {
            entity.HasMany(pc => pc.Deductions)
                .WithOne(d => d.PayableCharge)
                .HasForeignKey(d => d.PayableChargeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(pc => pc.Fees)
                .WithOne(f => f.PayableCharge)
                .HasForeignKey(f => f.PayableChargeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(pc => pc.Invoice)
                .WithOne(i => i.PayableCharge)
                .HasForeignKey<Invoice>(i => i.PayableChargeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(pc => pc.Taxes)
                .WithOne(t => t.PayableCharge)
                .HasForeignKey(t => t.PayableChargeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(pc => pc.Tags)
                .WithOne(t => t.PayableCharge)
                .HasForeignKey(t => t.PayableChargeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Add indexes for better query performance
        modelBuilder.Entity<SherwebModel>().HasIndex(c => c.DisplayName);
        modelBuilder.Entity<PayableCharge>().HasIndex(pc => pc.ChargeId);
        modelBuilder.Entity<Invoice>().HasIndex(i => i.Number);
    }
}
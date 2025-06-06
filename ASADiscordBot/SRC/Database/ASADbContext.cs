﻿using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ASADiscordBot.Database.Model;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
namespace ASADiscordBot.Database;

public class ASADbContext : DbContext
{
    public ASADbContext(DbContextOptions<ASADbContext> options) : base(options) { }
    
    public DbSet<Identity> Identities { get; set; }
    public DbSet<Reminder> Reminders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Identity>()
            .Property(b => b.IsAdmin)
            .HasDefaultValue(false);

        modelBuilder.Entity<Identity>()
            .Property(b => b.IsWhitelisted)
            .HasDefaultValue(false);

        modelBuilder.Entity<Identity>()
            .Property(b => b.IsSuperAdmin)
            .HasDefaultValue(false);
        
    }
}
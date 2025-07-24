using Microsoft.EntityFrameworkCore;
using TelegramBot.Application.Entities;

namespace TelegramBot.Infrastructure.Contexts;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }
    
    public DbSet<User> Users { get; set; }
    public DbSet<Operation> Operations { get; set; }
    public DbSet<OperationHistory> OperationHistories { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.TelegramId)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasMany(u => u.Operations)
            .WithOne(o => o.User!)
            .HasForeignKey(o => o.UserId);

        modelBuilder.Entity<Operation>()
            .HasMany(o => o.History)
            .WithOne(h => h.Operation!)
            .HasForeignKey(h => h.OperationId);
    }
}
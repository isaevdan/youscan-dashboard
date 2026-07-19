using Dashboard.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard.Infrastructure.Persistence;

public class DashboardDbContext : DbContext
{
    public DashboardDbContext(DbContextOptions<DashboardDbContext> options) : base(options)
    {
    }

    public DbSet<Widget> Widgets => Set<Widget>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Widget>(builder =>
        {
            builder.HasKey(w => w.Id);
            builder.Property(w => w.Type).HasConversion<string>().HasMaxLength(20).IsRequired();
            builder.Property(w => w.Order).IsRequired();
            builder.Property(w => w.DataJson).IsRequired();

            // Order is the grid position; uniqueness turns concurrent-create races
            // into a constraint violation the create handler can retry on.
            builder.HasIndex(w => w.Order).IsUnique();
        });
    }
}

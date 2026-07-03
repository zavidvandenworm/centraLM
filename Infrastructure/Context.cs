using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class Context(DbContextOptions<Context> options): DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>()
            .HasOne(g => g.ParentGroup)
            .WithMany(g => g.Children)
            .HasForeignKey(g => g.ParentGroupId);
        
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<VirtualKey> VirtualKeys { get; set; }
}
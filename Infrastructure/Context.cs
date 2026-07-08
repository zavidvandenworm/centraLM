using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure;

public class Context(DbContextOptions<Context> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Group> Groups { get; set; }
    public DbSet<GroupMember> GroupMembers { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<VirtualKey> VirtualKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Group>()
            .HasOne(g => g.ParentGroup)
            .WithMany(g => g.Children)
            .HasForeignKey(g => g.ParentGroupId);

        modelBuilder.Entity<GroupMember>()
            .HasOne(g => g.User)
            .WithMany()
            .HasForeignKey(g => g.UserId)
            .HasPrincipalKey(g => g.Id);

        modelBuilder.Entity<User>().HasKey(u => new { u.OpenIdIssuer, u.OpenIdSubject });
    }
}
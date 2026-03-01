using Dkef.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public sealed class RefreshTokenContext : DbContext
{
    public RefreshTokenContext(DbContextOptions<RefreshTokenContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RefreshToken>().HasKey(t => t.Id);
        modelBuilder.Entity<RefreshToken>().HasIndex(t => t.Token).IsUnique();
        
        // Configure the relationship to point to AspNetUsers table
        modelBuilder.Entity<RefreshToken>()
            .HasOne(rt => rt.Contact)
            .WithMany()
            .HasForeignKey(rt => rt.ContactId)
            .HasPrincipalKey(c => c.Id);
            
        // Tell EF Core that Contact entity is actually stored in AspNetUsers table
        modelBuilder.Entity<Contact>().ToTable("AspNetUsers");
    }
}

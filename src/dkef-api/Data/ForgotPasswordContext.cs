using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public sealed class ForgotPasswordContext : DbContext
{
    public ForgotPasswordContext(DbContextOptions<ForgotPasswordContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<ForgotPassword> ForgotPasswords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ForgotPassword>().HasKey(t => t.Id);
        
        // Configure the relationship to point to AspNetUsers table instead of Contact
        modelBuilder.Entity<ForgotPassword>()
            .HasOne(fp => fp.Contact)
            .WithMany()
            .HasForeignKey(fp => fp.ContactId)
            .HasPrincipalKey(c => c.Id);
            
        // Tell EF Core that Contact entity is actually stored in AspNetUsers table
        modelBuilder.Entity<Contact>().ToTable("AspNetUsers");
    }
}
using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public sealed class ChangeEmailContext : DbContext
{
    public ChangeEmailContext(DbContextOptions<ChangeEmailContext> options) : base(options)
    {
    }

    public DbSet<ChangeEmail> ChangeEmails { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChangeEmail>().HasKey(t => t.Id);

        // Configure the relationship to point to AspNetUsers table instead of Contact
        modelBuilder.Entity<ChangeEmail>()
            .HasOne(fp => fp.Contact)
            .WithMany()
            .HasForeignKey(fp => fp.ContactId)
            .HasPrincipalKey(c => c.Id);

        // Tell EF Core that Contact entity is actually stored in AspNetUsers table
        // but is owned/migrated by ContactContext — exclude from this context's migrations
        modelBuilder.Entity<Contact>().ToTable("AspNetUsers", t => t.ExcludeFromMigrations());
    }
}

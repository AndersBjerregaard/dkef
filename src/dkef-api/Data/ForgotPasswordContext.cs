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
    }
}
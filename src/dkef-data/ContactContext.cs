using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public class ContactContext : IdentityDbContext<Contact>
{

    public DbSet<Contact> Contacts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Username=dkef;Password=mysecretpassword;Database=dkef");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder); // IMPORTANT: must call base method to let Identity set up its own tables

        modelBuilder.Entity<Contact>().HasKey(t => t.Id);
    }
}

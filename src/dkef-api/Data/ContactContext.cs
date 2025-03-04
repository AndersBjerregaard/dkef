using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public class ContactContext(DbContextOptions<ContactContext> options) : DbContext(options) {

    public DbSet<Contact> Contacts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Contact>().HasKey(t => t.Id);
    }
}
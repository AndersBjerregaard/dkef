using Dkef.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public class AttachmentsContext : DbContext
{
    public AttachmentsContext(DbContextOptions<AttachmentsContext> options) : base(options)
    {
    }

    public DbSet<Attachment> Attachments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Attachment>().HasKey(t => t.Id);
    }
}

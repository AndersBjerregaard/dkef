using Dkef.Domain;
using Dkef.Domain.Abstracts;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public sealed class ContentsContext(DbContextOptions<ContentsContext> options)
: DbContext(options)
{
    public DbSet<BaseContent> Contents { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<EventSignUp> EventSignUps { get; set; }
    public DbSet<News> News { get; set; }
    public DbSet<GeneralAssembly> GeneralAssemblies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaseContent>().ToTable("Contents")
            .HasDiscriminator<string>("ContentType")
            .HasValue<Event>("Event")
            .HasValue<News>("News")
            .HasValue<GeneralAssembly>("GeneralAssembly");

        modelBuilder.Entity<EventSignUp>(entity =>
        {
            entity.ToTable("EventSignUps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ContactId).IsRequired();
            entity.Property(x => x.SignedUpAt).IsRequired();

            entity.HasOne(x => x.Event)
                .WithMany(x => x.SignUps)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.EventId, x.ContactId }).IsUnique();
            entity.HasIndex(x => x.ContactId);
        });
    }
}

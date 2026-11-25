using Dkef.Domain;
using Dkef.Domain.Abstracts;
using Dkef.Services;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public sealed class ContentsContext(DbContextOptions<ContentsContext> options)
: DbContext(options)
{
    public DbSet<BaseContent> Contents { get; set; }
    public DbSet<Event> Events { get; set; }
    public DbSet<News> News { get; set; }
    public DbSet<GeneralAssembly> GeneralAssemblies { get; set; }
    public DbSet<SiteText> SiteTexts { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BaseContent>().ToTable("Contents")
            .HasDiscriminator<string>("ContentType")
            .HasValue<Event>("Event")
            .HasValue<News>("News")
            .HasValue<GeneralAssembly>("GeneralAssembly");

        modelBuilder.Entity<SiteText>(entity =>
        {
            entity.ToTable("SiteTexts");
            entity.HasKey(text => new { text.Group, text.Key });
            entity.Property(text => text.Group).HasColumnName("Group")
                .HasMaxLength(SiteTextRegistry.MaxIdentifierLength).IsRequired();
            entity.Property(text => text.Key).HasColumnName("Key")
                .HasMaxLength(SiteTextRegistry.MaxIdentifierLength).IsRequired();
            entity.Property(text => text.Value).HasColumnName("Value").HasColumnType("text").IsRequired();
            entity.Property(text => text.UpdatedAt).HasColumnName("UpdatedAt")
                .HasColumnType("timestamp with time zone").IsRequired();
            entity.Property(text => text.UpdatedBy).HasColumnName("UpdatedBy").HasColumnType("text").IsRequired();
        });
    }
}

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
    public DbSet<SiteImage> SiteImages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<SiteImage>(entity =>
        {
            entity.ToTable("SiteImages");
            entity.HasKey(image => new { image.Group, image.Slot });
            entity.Property(image => image.Group).HasMaxLength(128);
            entity.Property(image => image.Slot).HasMaxLength(128);
            entity.Property(image => image.ObjectKey).HasMaxLength(256).IsRequired();
            entity.Property(image => image.Alt).HasMaxLength(300).IsRequired();
            entity.Property(image => image.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.Property(image => image.UpdatedBy).IsRequired();
        });

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

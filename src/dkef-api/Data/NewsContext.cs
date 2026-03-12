using Dkef.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public class NewsContext : DbContext
{
    public NewsContext(DbContextOptions<NewsContext> options) : base(options)
    {
    }

    public DbSet<News> News { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<News>().HasKey(t => t.Id);
    }
}

using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public class EventsContext : DbContext
{
    public EventsContext(DbContextOptions<EventsContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Event>().HasKey(t => t.Id);
    }
}
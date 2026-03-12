using Dkef.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public class GeneralAssemblyContext : DbContext
{
    public GeneralAssemblyContext(DbContextOptions<GeneralAssemblyContext> options) : base(options)
    {
    }

    public DbSet<GeneralAssembly> GeneralAssemblies { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<GeneralAssembly>().HasKey(t => t.Id);
    }
}

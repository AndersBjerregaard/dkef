using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public sealed class AspNetRolesContext(
    DbContextOptions<AspNetRolesContext> options
) : DbContext(options)
{
    public DbSet<IdentityRole> AspNetRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IdentityRole>()
        .ToTable("AspNetRoles")
        .HasData(
            new IdentityRole
            {
                Id = "5a7892eb-6f59-4753-9ec4-3d173010647f",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "ca6e84e2-b7c4-4de6-be47-ee673fa69b69"
            },
            new IdentityRole
            {
                Id = "c6850c95-b8a1-4491-a52f-83b47b87b9cd",
                Name = "Board Member",
                NormalizedName = "BOARD MEMBER",
                ConcurrencyStamp = "7f1a1f5a-3b14-434b-8647-7c5190f3f425"
            }
        );
    }
}

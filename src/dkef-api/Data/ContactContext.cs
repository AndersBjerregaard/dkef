using Bogus;

using Dkef.Domain;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public class ContactContext : IdentityDbContext<Contact>
{

    public ContactContext(DbContextOptions<ContactContext> options) : base(options)
    {
    }

    public DbSet<Contact> Contacts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {

        base.OnModelCreating(modelBuilder); // IMPORTANT: must call base method to let Identity set up its own tables

        modelBuilder.Entity<Contact>().HasKey(t => t.Id);
    }

    public static async Task SeedAsync(ContactContext context)
    {

        var faker = new Faker<Contact>().UseSeed(69)
            // Contact specific properties
            .RuleFor(x => x.Id, y => Guid.NewGuid().ToString())
            .RuleFor(x => x.Email, y => y.Person.Email)
            .RuleFor(x => x.Name, y => y.Person.FullName)
            .RuleFor(x => x.Title, y => y.Hacker.Noun())
            .RuleFor(x => x.EmploymentStatus, y => y.Hacker.Noun())
            .RuleFor(x => x.Address, y => y.Address.StreetAddress())
            .RuleFor(x => x.ZIP, y => y.Address.ZipCode())
            .RuleFor(x => x.City, y => y.Address.City())
            .RuleFor(x => x.PhoneNumber, y => y.Person.Phone)
            .RuleFor(x => x.CompanyName, y => y.Company.CompanyName())
            .RuleFor(x => x.CompanyAddress, y => y.Address.StreetAddress())
            .RuleFor(x => x.CompanyZIP, y => y.Address.ZipCode())
            .RuleFor(x => x.CompanyCity, y => y.Address.City())
            .RuleFor(x => x.CVRNumber, y => y.Random.Int(10000000, 99999999).ToString())
            .RuleFor(x => x.CompanyPhone, y => y.Phone.PhoneNumber())
            .RuleFor(x => x.MagazineDelivery, y => "Privatadresse")
            .RuleFor(x => x.EANNumber, y => y.Random.Int(10000000, 99999999).ToString())
            .RuleFor(x => x.PrimarySection, y => y.Random.Bool() ? Dkef.Domain.Section.Funen : Dkef.Domain.Section.Jutland)
            .RuleFor(x => x.SecondarySection, y => y.Random.Bool() ? Dkef.Domain.Section.Funen : Dkef.Domain.Section.Jutland)
            .RuleFor(x => x.EnrollmentDate, y => y.Date.Recent())
            // IdentityUser specific properties
            .RuleFor(x => x.UserName, (y, x) => x.Email)
            .RuleFor(x => x.NormalizedUserName, (y, x) => x.Email)
            .RuleFor(x => x.NormalizedEmail, (y, x) => x.Email)
            .RuleFor(x => x.EmailConfirmed, y => true)
            .RuleFor(x => x.PasswordHash, (x, y) => new PasswordHasher<Contact>().HashPassword(y, "Password123!"))
            .RuleFor(x => x.SecurityStamp, y => Guid.NewGuid().ToString())
            .RuleFor(x => x.ConcurrencyStamp, y => Guid.NewGuid().ToString())
            .RuleFor(x => x.PhoneNumber, (y, x) => x.PhoneNumber)
            .RuleFor(x => x.PhoneNumberConfirmed, y => true);
        var contacts = faker.Generate(600);

        // Needs to use cascading truncate
        // await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{RELATION_NAME}\";");

        await context.Contacts.AddRangeAsync(contacts);
        await context.SaveChangesAsync();
    }
}

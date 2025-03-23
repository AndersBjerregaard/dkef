using Bogus;
using Dkef.Domain;

using Microsoft.EntityFrameworkCore;

namespace Dkef.Data;

public class ContactContext: DbContext {
    
    public ContactContext(DbContextOptions<ContactContext> options) : base(options) {
        Database.EnsureCreated();
    }

    public DbSet<Contact> Contacts { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.Entity<Contact>().HasKey(t => t.Id);
    }

    public static async Task SeedAsync(ContactContext context) {

        var faker = new Faker<Contact>().UseSeed(69)
            .RuleFor(x => x.Email, y => y.Person.Email)
            .RuleFor(x => x.FirstName, y => y.Person.FirstName)
            .RuleFor(x => x.LastName, y => y.Person.LastName)
            .RuleFor(x => x.Title, y => y.Hacker.Noun())
            .RuleFor(x => x.Occupation, y => y.Hacker.Noun())
            .RuleFor(x => x.WorkTasks, y => y.Hacker.Phrase())
            .RuleFor(x => x.PrivateAddress, y => y.Address.StreetAddress())
            .RuleFor(x => x.PrivateZIP, y => y.Address.ZipCode())
            .RuleFor(x => x.PrivateCity, y => y.Address.City())
            .RuleFor(x => x.PrivatePhone, y => y.Person.Phone)
            .RuleFor(x => x.CompanyName, y => y.Company.CompanyName())
            .RuleFor(x => x.CompanyAddress, y => y.Address.StreetAddress())
            .RuleFor(x => x.CompanyZIP, y => y.Address.ZipCode())
            .RuleFor(x => x.CompanyCity, y => y.Address.City())
            .RuleFor(x => x.CVRNumber, y => y.Random.Int(10000000, 99999999).ToString())
            .RuleFor(x => x.CompanyPhone, y => y.Phone.PhoneNumber())
            .RuleFor(x => x.CompanyEmail, y => y.Internet.Email())
            .RuleFor(x => x.ElTeknikDelivery, y => "Privatadresse")
            .RuleFor(x => x.EANNumber, y => y.Random.Int(10000000, 99999999).ToString())
            .RuleFor(x => x.Invoice, y => "Privat")
            .RuleFor(x => x.HelpToStudents, y => y.Random.Bool() ? "Ja" : "Nej")
            .RuleFor(x => x.Mentor, y => y.Random.Bool() ? "Ja" : "Nej")
            .RuleFor(x => x.PrimarySection, y => y.Random.Bool() ? "Fynsk Sektion" : "Jydsk Sektion")
            .RuleFor(x => x.SecondarySection, y => y.Random.Bool() ? "Nordjysk Sektion" : "Sydjydsk Sektion")
            .RuleFor(x => x.InvoiceEmail, y => y.Person.Email)
            .RuleFor(x => x.OldMemberNumber, y => y.Random.Int(10000000, 99999999).ToString())
            .RuleFor(x => x.RegistrationDate, y => y.Date.RecentDateOnly().ToString())
            .RuleFor(x => x.ATTInvoice, y => "Nej")
            .RuleFor(x => x.Source, y => "V1 Medlemsportal")
            .RuleFor(x => x.ExpectedEndDateOfBeingStudent, y => string.Empty);
        var contacts = faker.Generate(20);

        await context.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE \"{nameof(Contacts)}\";");
        await context.Contacts.AddRangeAsync(contacts);
        await context.SaveChangesAsync();
    }
}
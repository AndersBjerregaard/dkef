using System.Formats.Tar;
using System.Globalization;
using Dkef.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

Console.WriteLine("Enter 'dinero', 'update'");
Console.Write("> ");

string? input = Console.ReadLine();

if (input is null ||
    input != "dinero" &&
    input != "update")
{
    Console.WriteLine("Invalid input.");
    return;
}

Console.WriteLine("Connecting to database...");

using var db = new ContactsContext();

if (input.Equals("dinero", StringComparison.OrdinalIgnoreCase))
{
    var contactsAmount = await db.Contacts.CountAsync();

    if (contactsAmount != 0)
    {
        Console.WriteLine("Contacts table is not empty! Terminating...");
        return;
    }
    Dictionary<string, Contact> contactsToAdd = new();

    Console.WriteLine("Reading dinero data...");

    string filePath = "data/dinero_members.csv";

    using var reader = new StreamReader(filePath);

    // Skip first line since they're just headers
    reader.ReadLine();

    string? line;
    uint index = 0;

    var passwordHasher = new PasswordHasher<Contact>();

    while ((line = reader.ReadLine()) is not null)
    {
        index++;
        string[] fields = line.Split(';', StringSplitOptions.None);

        if (fields.Length != 16)
        {
            Console.WriteLine($"Line {index}: Expected 16 fields, got {fields.Length}");
            continue;
        }

        if (fields[15] != "Private")
        {
            Console.WriteLine($"Skipping non-private member on line {index}");
            continue;
        }

        if (string.IsNullOrEmpty(fields[8]))
        {
            Console.WriteLine($"Skipping contact with no email on line {index}");
            continue;
        }

        var contact = new Contact
        {
            // Domain fields
            Name = fields[0],
            Address = fields[1],
            ZIP = fields[2],
            City = fields[3],
            CountryCode = fields[4],
            CVRNumber = fields[5],
            EANNumber = fields[6],
            PrivatePhoneNumber = fields[7],
            Email = fields[8],
            AttPerson = fields[9],
            PaymentMethod = fields[11],
            PaymentDeadlineInDays = uint.Parse(fields[12]),
            TotalSale = fields[13],
            TotalPurchase = fields[14],
            // IdentityUser Fields
            UserName = fields[8],
            NormalizedUserName = fields[8].ToUpper(),
            NormalizedEmail = fields[8].ToUpper(),
            EmailConfirmed = false,
            SecurityStamp = Guid.NewGuid().ToString(),
            ConcurrencyStamp = Guid.NewGuid().ToString(),
            PhoneNumber = fields[7],
            PhoneNumberConfirmed = false,
        };

        var passwordHash = passwordHasher.HashPassword(contact, Guid.NewGuid().ToString());

        contact.PasswordHash = passwordHash;

        bool added = contactsToAdd.TryAdd(contact.NormalizedUserName, contact);
        if (!added)
        {
            Console.WriteLine($"Skipping duplicate contact on line {index + 1}");
        }
    }

    await db.Contacts.AddRangeAsync(contactsToAdd.Select(x => x.Value));
    await db.SaveChangesAsync();
    Console.WriteLine($"Added {contactsToAdd.Count} new contacts");
}

if (input.Equals("update", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine("Write a property key selector: 'email' or 'name'");
    Console.Write("> ");
    input = Console.ReadLine();

    if (input is null ||
        !(input.Equals("email", StringComparison.OrdinalIgnoreCase) ||
          input.Equals("name", StringComparison.OrdinalIgnoreCase)))
    {
        Console.WriteLine("Invalid property key selector");
        return;
    }

    string keySelector = input.ToLower() == "email" ? "email" : "name";

    Dictionary<string, Contact> contactsToUpdate = new();

    string filePath = "data/dkef_fynsk_medlemmer.csv";

    ReadFile(keySelector, contactsToUpdate, filePath);

    filePath = "data/dkef_jydsk_medlemmer.csv";

    ReadFile(keySelector, contactsToUpdate, filePath);

    filePath = "data/dkef_kbh_medlemmer.csv";

    ReadFile(keySelector, contactsToUpdate, filePath);

    filePath = "data/dkef_nordjysk_medlemmer.csv";

    ReadFile(keySelector, contactsToUpdate, filePath);

    filePath = "data/dkef_sydjysk_medlemmer.csv";

    ReadFile(keySelector, contactsToUpdate, filePath);

    Console.WriteLine($"Loaded {contactsToUpdate.Count} contacts from all files.");

    // Find contacts in the database that match the keys in contactsToUpdate
    // Contains the existing database contacts as keys, and the corresponding contacts from the files as values
    Dictionary<Contact, Contact> contactsApplicableForUpdate = new();

    if (keySelector == "name")
    {
        foreach (KeyValuePair<string, Contact> kv in contactsToUpdate)
        {
            // Retrieve with tracking for updating
            List<Contact> hits = db.Contacts
                .Where(x => x.Name == kv.Key)
                .ToList();
            if (hits.Count > 1)
            {
                Console.WriteLine($"Multiple hits for {kv.Key}: {string.Join(", ", hits.Select(x => x.Name))}");
                continue;
            }
            if (hits.Count == 0)
            {
                Console.WriteLine($"No hit for {kv.Key}");
                continue;
            }
            contactsApplicableForUpdate.Add(hits[0], kv.Value);
        }
        Console.WriteLine($"Found {contactsApplicableForUpdate.Count} contacts to update");
    }

    if (keySelector == "email")
    {
        foreach (KeyValuePair<string, Contact> kv in contactsToUpdate)
        {
            // Retrieve with tracking for updating
            List<Contact> hits = db.Contacts
                .Where(x => x.Email == kv.Key)
                .ToList();
            if (hits.Count > 1)
            {
                Console.WriteLine($"Multiple hits for {kv.Key}: {string.Join(", ", hits.Select(x => x.Name))}");
                continue;
            }
            if (hits.Count == 0)
            {
                Console.WriteLine($"No hit for {kv.Key}");
                continue;
            }
            contactsApplicableForUpdate.Add(hits[0], kv.Value);
        }
        Console.WriteLine($"Found {contactsApplicableForUpdate.Count} contacts to update");
    }

    Console.WriteLine("Proceed with update? (y/n)");

    string? proceed = Console.ReadLine();
    if (proceed?.ToLower() != "y")
    {
        Console.WriteLine("Update cancelled");
        return;
    }

    // Update the contacts in the database
    foreach (var (existingContact, updatedContact) in contactsApplicableForUpdate)
    {
        // Map property values from updatedContact to existingContact
        // Preserve all property values on existingContact that are not default
        MapContacts(existingContact, updatedContact);
        db.SaveChanges();
    }

    Console.WriteLine($"Successfully updated {contactsApplicableForUpdate.Count} contacts");
}

static void ReadFile(string keySelector, Dictionary<string, Contact> contactsToUpdate, string filePath)
{
    Console.WriteLine($"Reading '{filePath}' ...");
    var reader = new StreamReader(filePath);

    // Skip first line since they're just headers
    reader.ReadLine();

    string? line;
    uint index = 0;

    while ((line = reader.ReadLine()) is not null)
    {
        index++;
        string[] fields = line.Split(';', StringSplitOptions.None);

        bool dateParsed = DateTime.TryParseExact(
            fields[4],
            "M/d/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out DateTime enrollmentDate);

        DateTime utcDate = DateTime.SpecifyKind(enrollmentDate, DateTimeKind.Utc);

        string? sectionString = fields[17];
        Section? primarySection = null;
        if (!string.IsNullOrEmpty(sectionString))
        {
            if (sectionString.Equals("Fynsk Sektion", StringComparison.OrdinalIgnoreCase))
            {
                primarySection = Section.Funen;
            }
            else if (sectionString.Equals("Jydsk Sektion", StringComparison.OrdinalIgnoreCase))
            {
                primarySection = Section.Jutland;
            }
            else if (sectionString.Equals("Nordjysk Sektion", StringComparison.OrdinalIgnoreCase))
            {
                primarySection = Section.NorthJutland;
            }
            else if (sectionString.Equals("Sydjysk Sektion", StringComparison.OrdinalIgnoreCase))
            {
                primarySection = Section.SouthJutland;
            }
            else if (sectionString.Equals("København Sjælland", StringComparison.OrdinalIgnoreCase))
            {
                primarySection = Section.CopenhagenZealand;
            }
            else if (sectionString.Equals("Hovedforeningen", StringComparison.OrdinalIgnoreCase))
            {
                primarySection = Section.MainAssociation;
            }
        }
        string? secondarySectionString = fields[18];
        Section? secondarySection = null;
        if (!string.IsNullOrEmpty(secondarySectionString))
        {
            if (secondarySectionString.Equals("Fynsk Sektion", StringComparison.OrdinalIgnoreCase))
            {
                secondarySection = Section.Funen;
            }
            else if (secondarySectionString.Equals("Jydsk Sektion", StringComparison.OrdinalIgnoreCase))
            {
                secondarySection = Section.Jutland;
            }
            else if (secondarySectionString.Equals("Nordjysk Sektion", StringComparison.OrdinalIgnoreCase))
            {
                secondarySection = Section.NorthJutland;
            }
            else if (secondarySectionString.Equals("Sydjysk Sektion", StringComparison.OrdinalIgnoreCase))
            {
                secondarySection = Section.SouthJutland;
            }
            else if (secondarySectionString.Equals("København Sjælland", StringComparison.OrdinalIgnoreCase))
            {
                secondarySection = Section.CopenhagenZealand;
            }
            else if (secondarySectionString.Equals("Hovedforeningen", StringComparison.OrdinalIgnoreCase))
            {
                secondarySection = Section.MainAssociation;
            }
        }

        string? memberTypeString = fields[21];
        MemberType memberType = MemberType.Member;

        if (memberTypeString is not null && memberTypeString.Equals("Bestyrelse", StringComparison.OrdinalIgnoreCase))
        {
            memberType = MemberType.BoardMember;
        }

        var contact = new Contact
        {
            Name = fields[0],
            Address = fields[1],
            ZIP = fields[2],
            City = fields[3],
            EnrollmentDate = dateParsed ? utcDate : null,
            Email = fields[5],
            Subscription = fields[6],
            PrivatePhoneNumber = fields[7],
            CompanyName = fields[8],
            InvoiceName2 = fields[9],
            CompanyAddress = fields[10],
            CompanyZIP = fields[11],
            CompanyCity = fields[12],
            // Fakt e-mail 13
            EANNumber = fields[14],
            CVRNumber = fields[15],
            EmploymentStatus = fields[16],
            PrimarySection = primarySection,
            SecondarySection = secondarySection,
            MagazineDelivery = fields[19],
            Title = fields[20],
            MemberType = memberType,
            CompanyPhone = fields[22]
        };

        if (keySelector == "name")
        {
            if (string.IsNullOrEmpty(contact.Name))
            {
                Console.WriteLine($"Skipping contact with no name on line {index + 1}");
                continue;
            }
            string key = contact.Name;
            if (!contactsToUpdate.TryAdd(key, contact))
            {
                Console.WriteLine($"Duplicate contact: {key}");
            }
        }

        if (keySelector == "email")
        {
            if (string.IsNullOrEmpty(contact.Email))
            {
                Console.WriteLine($"Skipping contact with no email on line {index + 1}");
                continue;
            }
            string key = contact.Email;
            if (!contactsToUpdate.TryAdd(key, contact))
            {
                Console.WriteLine($"Duplicate contact: {key}");
            }
        }
    }
}

/// <summary>
/// Maps the properties of the source contact to the target contact.
/// But only updates the properties on the target contact that are not default
/// </summary>
static void MapContacts(Contact target, Contact source)
{
    if (source == null) return;
    if (target == null) return;

    if (string.IsNullOrEmpty(target.Email))
    {
        target.Email = source.Email;
        target.UserName = source.Email;
        target.NormalizedEmail = source.Email?.ToUpperInvariant();
        target.NormalizedUserName = source.Email?.ToUpperInvariant();
    }

    if (string.IsNullOrEmpty(target.Name))
    {
        target.Name = source.Name ?? "";
    }

    if (string.IsNullOrEmpty(target.Address))
    {
        target.Address = source.Address ?? "";
    }

    if (string.IsNullOrEmpty(target.ZIP))
    {
        target.ZIP = source.ZIP ?? "";
    }

    if (string.IsNullOrEmpty(target.City))
    {
        target.City = source.City ?? "";
    }

    if (string.IsNullOrEmpty(target.CountryCode))
    {
        target.CountryCode = source.CountryCode ?? "";
    }

    if (string.IsNullOrEmpty(target.CVRNumber))
    {
        target.CVRNumber = source.CVRNumber ?? "";
    }

    if (string.IsNullOrEmpty(target.EANNumber))
    {
        target.EANNumber = source.EANNumber ?? "";
    }

    if (string.IsNullOrEmpty(target.PrivatePhoneNumber))
    {
        target.PrivatePhoneNumber = source.PrivatePhoneNumber ?? "";
    }

    if (string.IsNullOrEmpty(target.PrivatePhoneNumber))
    {
        target.PrivatePhoneNumber = source.PrivatePhoneNumber ?? "";
    }

    if (string.IsNullOrEmpty(target.AttPerson))
    {
        target.AttPerson = source.AttPerson ?? "";
    }

    if (string.IsNullOrEmpty(target.PaymentMethod))
    {
        target.PaymentMethod = source.PaymentMethod ?? "";
    }

    if (string.IsNullOrEmpty(target.TotalSale))
    {
        target.TotalSale = source.TotalSale ?? "";
    }

    if (string.IsNullOrEmpty(target.TotalPurchase))
    {
        target.TotalPurchase = source.TotalPurchase ?? "";
    }

    if (!target.EnrollmentDate.HasValue && source.EnrollmentDate.HasValue)
    {
        target.EnrollmentDate = source.EnrollmentDate.Value;
    }

    if (string.IsNullOrEmpty(target.Subscription))
    {
        target.Subscription = source.Subscription ?? "";
    }

    if (string.IsNullOrEmpty(target.InvoiceName2))
    {
        target.InvoiceName2 = source.InvoiceName2 ?? "";
    }

    if (string.IsNullOrEmpty(target.CompanyName))
    {
        target.CompanyName = source.CompanyName ?? "";
    }

    if (string.IsNullOrEmpty(target.CompanyAddress))
    {
        target.CompanyAddress = source.CompanyAddress ?? "";
    }

    if (string.IsNullOrEmpty(target.CompanyZIP))
    {
        target.CompanyZIP = source.CompanyZIP ?? "";
    }

    if (string.IsNullOrEmpty(target.CompanyCity))
    {
        target.CompanyCity = source.CompanyCity ?? "";
    }

    if (string.IsNullOrEmpty(target.CompanyPhone))
    {
        target.CompanyPhone = source.CompanyPhone ?? "";
    }

    if (string.IsNullOrEmpty(target.EmploymentStatus))
    {
        target.EmploymentStatus = source.EmploymentStatus ?? "";
    }

    if (!target.PrimarySection.HasValue && source.PrimarySection.HasValue)
    {
        target.PrimarySection = source.PrimarySection;
    }

    if (!target.SecondarySection.HasValue && source.SecondarySection.HasValue)
    {
        target.SecondarySection = source.SecondarySection;
    }

    if (string.IsNullOrEmpty(target.MagazineDelivery))
    {
        target.MagazineDelivery = source.MagazineDelivery ?? "";
    }

    if (string.IsNullOrEmpty(target.Title))
    {
        target.Title = source.Title ?? "";
    }

    if (target.MemberType != source.MemberType)
    {
        target.MemberType = source.MemberType;
    }
}

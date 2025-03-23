using Dkef.Data;
using Dkef.Domain;
using Microsoft.EntityFrameworkCore;

namespace Dkef.Endpoints;

public class ContactEndpoints : BaseEndpoints {
    public override void MapEndpoints(WebApplication app)
    {
        //// Seeding endpoint
        // app.MapGet("/contacts/seed", async (ContactContext context) => {
        //     string filePath = "denero_contacts.csv";
        //     using var reader = new StreamReader(filePath);
        //     // Skip first line since they're just headers
        //     await reader.ReadLineAsync();
        //     string? line;
        //     string[] values;
        //     List<Contact> contacts = [];
        //     while ((line = await reader.ReadLineAsync()) != null) {
        //         Console.WriteLine(line);
        //         values = [.. Enumerable.Repeat(string.Empty, 39)];
        //         int i = 0;
        //         int column = 0;
        //         bool quoteChar = false;
        //         string columnValue = string.Empty;
        //         while (i < line.Length) {
        //             char character = line[i];
        //             if (character == '"') {
        //                 quoteChar = !quoteChar;
        //             }
        //             if (character == ',' && !quoteChar) {
        //                 values[column] = columnValue;
        //                 column++;
        //                 columnValue = string.Empty;
        //             } else if (character != '"') {
        //                 columnValue += character;
        //             }
        //             i++;
        //         }
        //         Contact contact = new() {
        //             Id = Guid.NewGuid(),
        //             Email = values[1],
        //             FirstName = values[9],
        //             LastName = values[10],
        //             Title = values[11],
        //             Occupation = values[12],
        //             WorkTasks = values[13],
        //             PrivateAddress = values[14],
        //             PrivateZIP = values[15],
        //             PrivateCity = values[16],
        //             PrivatePhone = values[17],
        //             CompanyName = values[18],
        //             CompanyAddress = values[19],
        //             CompanyZIP = values[20],
        //             CompanyCity = values[21],
        //             CVRNumber = values[22],
        //             CompanyPhone = values[23],
        //             CompanyEmail = values[24],
        //             ElTeknikDelivery = values[25],
        //             EANNumber = values[26],
        //             Invoice = values[27],
        //             HelpToStudents = values[29],
        //             Mentor = values[30],
        //             PrimarySection = values[31],
        //             SecondarySection = values[32],
        //             InvoiceEmail = values[33],
        //             OldMemberNumber = values[34],
        //             RegistrationDate = values[35],
        //             ATTInvoice = values[36],
        //             Source = values[37],
        //             ExpectedEndDateOfBeingStudent = values[38] ?? string.Empty
        //         };
        //         contacts.Add(contact);
        //     }
        //     Console.WriteLine($"Loaded {contacts.Count} contacts");
        //     await context.AddRangeAsync(contacts);
        //     var changed = await context.SaveChangesAsync();
        //     return $"Changes: {changed}";
        // })
        // .WithName("Seed");
    }
}
namespace Dkef.Domain;

// Member ID,email,CreatedAt,Verified,Stripe Customer ID,Free Plans (plan ids),Paid Plans (price ids),Login Redirect,Last Login,Fornavn,Efternavn,Titel,Beskæftigelse,Arbejdsopgaver,Privat vejnavn og nr,Privat postnummer,Privat by,Privat mobil,Firma navn,Firma vejnavn og nr.,Firma postnummer,Firma by,CVR nr.,Firma mobil,Firma e-mail,El-teknik levering,EAN nr.,Fakturering,GDPR accept,Hjælp til studerende,Mentor,Primær sektion,Sekundær sektion,Faktura e-mail,Gammelt Medlemsnummer,Tilmeldingsdato,ATT Faktura,Kilde,Hvornår forventer du at være færdig som studerende?
public class Contact {
    public Guid Id { get; set; } // NaN
    public string Email { get; set; } = string.Empty; // 1
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow; // 2
    public string FirstName { get; set; } = string.Empty; // 9
    public string LastName { get; set; } = string.Empty; // 10
    public string Title { get; set; } = string.Empty; // 11
    public string Occupation { get; set; } = string.Empty; // 12
    public string WorkTasks { get; set; } = string.Empty; // 13
    public string PrivateAddress { get; set; } = string.Empty; // 14
    public string PrivateZIP { get; set; } = string.Empty; // 15
    public string PrivateCity { get; set; } = string.Empty; // 16
    public string PrivatePhone { get; set; } = string.Empty; // 17
    public string CompanyName { get; set; } = string.Empty; // 18
    public string CompanyAddress { get; set; } = string.Empty; // 19
    public string CompanyZIP { get; set; } = string.Empty; // 20
    public string CompanyCity { get; set; } = string.Empty; // 21
    public string CVRNumber { get; set; } = string.Empty; // 22
    public string CompanyPhone { get; set; } = string.Empty; // 23
    public string CompanyEmail { get; set; } = string.Empty; // 24
    public string ElTeknikDelivery { get; set; } = string.Empty; // 25
    public string EANNumber { get; set; } = string.Empty; // 26
    public string Invoice { get; set; } = string.Empty; // 27
    public string HelpToStudents { get; set; } = string.Empty; // 29
    public string Mentor { get; set; } = string.Empty; // 30
    public string PrimarySection { get; set; } = string.Empty; //31
    public string SecondarySection { get; set; } = string.Empty; // 32
    public string InvoiceEmail { get; set; } = string.Empty; // 33
    public string OldMemberNumber { get; set; } = string.Empty; // 34
    public string RegistrationDate { get; set; } = string.Empty; // 35
    public string ATTInvoice { get; set; } = string.Empty; // 36
    public string Source { get; set; } = string.Empty; // 37
    public string ExpectedEndDateOfBeingStudent { get; set; } = string.Empty; // 38
}
# NewMember.razor Email Template

## Overview

The `NewMember.razor` template is used to notify the secretary when a new member registers via the website. It intelligently displays all provided registration information while hiding empty optional fields.

## Key Features

- **Conditional Rendering**: Only displays fields that have values
- **Organized Sections**: Information is grouped logically:
  - Personal Information (Name, Email, Phone, Title, Employment Status)
  - Address Information (if any address fields are provided)
  - Membership Information (Sections, Magazine Delivery, Subscription)
  - Company Information (if any company fields are provided)
- **Professional Styling**: Uses the shared `EmailBaseStyles.razor` for consistent appearance
- **Dark Theme**: Matches the organization's design system with amber accents

## Parameters

The template accepts the following parameters (all `string` except where noted):

### Required Fields
- `Name` (string) - Member's full name
- `Email` (string) - Member's email address
- `PrimarySection` (Section?) - Primary section enum value
- `MagazineDelivery` (string) - Magazine delivery preference
- `Subscription` (string) - Subscription preference
- `ReceivedAt` (string) - Timestamp of registration (formatted date/time)

### Optional Personal Information
- `Phone` (string) - Contact phone number
- `Title` (string) - Job title
- `EmploymentStatus` (string) - Employment status

### Optional Address Information
- `Address` (string) - Street address
- `ZIP` (string) - Postal code
- `City` (string) - City name

### Optional Membership Information
- `SecondarySection` (Section?) - Secondary section (nullable enum)

### Optional Company Information
- `CompanyName` (string) - Company name
- `CompanyAddress` (string) - Company street address
- `CompanyZIP` (string) - Company postal code
- `CompanyCity` (string) - Company city
- `CompanyPhone` (string) - Company phone number
- `CVRNumber` (string) - Danish business registration number
- `EANNumber` (string) - European article number

## Usage Example

In your email sending service, instantiate the component with registration data:

```csharp
var html = await renderer.Dispatcher.InvokeAsync(async () =>
{
    var component = new MarkupString(
        await renderer.RenderComponentAsync<NewMember>(
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                { nameof(NewMember.Name), registration.Name },
                { nameof(NewMember.Email), registration.Email },
                { nameof(NewMember.Phone), registration.Phone },
                { nameof(NewMember.Title), registration.Title },
                { nameof(NewMember.EmploymentStatus), registration.EmploymentStatus },
                { nameof(NewMember.Address), registration.Address },
                { nameof(NewMember.ZIP), registration.ZIP },
                { nameof(NewMember.City), registration.City },
                { nameof(NewMember.PrimarySection), registration.PrimarySection },
                { nameof(NewMember.SecondarySection), registration.SecondarySection },
                { nameof(NewMember.MagazineDelivery), registration.MagazineDelivery },
                { nameof(NewMember.Subscription), registration.Subscription },
                { nameof(NewMember.CompanyName), registration.CompanyName },
                { nameof(NewMember.CompanyAddress), registration.CompanyAddress },
                { nameof(NewMember.CompanyZIP), registration.CompanyZIP },
                { nameof(NewMember.CompanyCity), registration.CompanyCity },
                { nameof(NewMember.CompanyPhone), registration.CompanyPhone },
                { nameof(NewMember.CVRNumber), registration.CVRNumber },
                { nameof(NewMember.EANNumber), registration.EANNumber },
                { nameof(NewMember.ReceivedAt), DateTime.UtcNow.ToString("g") }
            })
        )
    );
});
```

## Behavior

### Conditional Sections

**Address Information Section** appears if any of these are provided:
- Address
- ZIP
- City

**Company Information Section** appears if any of these are provided:
- CompanyName
- CompanyAddress
- CompanyZIP
- CompanyCity
- CompanyPhone
- CVRNumber
- EANNumber

### Empty Value Handling

- Required fields are always displayed (Name, Email, PrimarySection, MagazineDelivery, Subscription)
- Optional fields use `string.IsNullOrWhiteSpace()` checks before rendering
- Nullable enum fields (sections) use `HasValue` checks
- Empty sections don't clutter the email output

## Styling

All styling is defined in `EmailBaseStyles.razor`. Key CSS classes:

- `.info-row` - Individual information row
- `.info-icon` - Icon/emoji container
- `.info-label` - Field label (uppercase amber)
- `.info-value` - Field value (light slate)
- `.section-title` - Section heading
- `.divider` - Visual separator between sections

All styles are optimized for email clients with responsive behavior for smaller screens.

## Emojis Used

The template uses semantic emojis for visual organization:
- 👤 Name
- ✉️ Email
- 📞 Phone
- 💼 Title
- 👨‍💼 Employment Status
- 🏠 Address
- 📮 Postal code
- 🏙️ City
- ⭐ Primary section
- ✓ Secondary section
- 📰 Magazine delivery
- 🔔 Subscription
- 🏭 Company name
- 🔢 CVR/EAN numbers

## Notes

- The template always includes a timestamp and source indicator
- Password and ConfirmPassword from RegisterDto are **never** included in the email
- All string values are sanitized before rendering (assumed to be done at the service layer)
- The template is optimized for all major email clients

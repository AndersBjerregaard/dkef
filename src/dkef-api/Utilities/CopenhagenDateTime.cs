using System.Globalization;

namespace Dkef.Utilities;

public static class CopenhagenDateTime
{
    // Local date strings without an explicit offset are treated as Denmark local time,
    // so runtime timezone data for Europe/Copenhagen must be present in production containers.
    private static readonly string[] LocalDateTimeFormats =
    [
        "yyyy-MM-ddTHH:mm",
        "yyyy-MM-ddTHH:mm:ss",
        "yyyy-MM-ddTHH:mm:ss.FFFFFFF"
    ];

    private static readonly TimeZoneInfo TimeZone = ResolveTimeZone();

    public static DateTime ParseToUtc(string value)
    {
        if (!TryParseToUtc(value, out var parsedUtc))
        {
            throw new FormatException("Invalid date time value.");
        }

        return parsedUtc;
    }

    public static bool TryParseToUtc(string? value, out DateTime parsedUtc)
    {
        parsedUtc = default;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        if (DateTime.TryParseExact(
            value,
            LocalDateTimeFormats,
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var localDateTime))
        {
            parsedUtc = TimeZoneInfo.ConvertTimeToUtc(
                DateTime.SpecifyKind(localDateTime, DateTimeKind.Unspecified),
                TimeZone);
            return true;
        }

        if (DateTimeOffset.TryParse(
            value,
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var offsetDateTime))
        {
            parsedUtc = offsetDateTime.UtcDateTime;
            return true;
        }

        return false;
    }

    private static TimeZoneInfo ResolveTimeZone()
    {
        string[] timezoneIds = ["Europe/Copenhagen", "Romance Standard Time", "Central Europe Standard Time"];

        foreach (var timezoneId in timezoneIds)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(timezoneId);
            }
            catch (TimeZoneNotFoundException)
            {
            }
            catch (InvalidTimeZoneException)
            {
            }
        }

        throw new InvalidOperationException("Could not resolve Europe/Copenhagen timezone.");
    }
}

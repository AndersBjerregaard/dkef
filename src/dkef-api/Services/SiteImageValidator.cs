using System.Buffers.Binary;
using ImageMagick;

namespace Dkef.Services;

public sealed class SiteImageValidationException(string message) : Exception(message);

public static class SiteImageValidator
{
    public const int MaxFileBytes = 5 * 1024 * 1024;
    public const int MaxRequestBytes = MaxFileBytes + 64 * 1024;
    public const int MaxAltLength = 300;
    private const long MaxPixels = 16_000_000;

    public static string ValidateAlt(string? alt)
    {
        // Same plain-text sanitization boundary as editable site text, never rich HTML.
        alt = alt is null ? null : SiteTextValueRules.Sanitize(alt, SiteTextValueRules.CreateSanitizer());
        alt = alt?.Trim();
        if (string.IsNullOrWhiteSpace(alt) || alt.Length > MaxAltLength ||
            alt.Any(character => char.IsControl(character) || character is '\u2028' or '\u2029'))
            throw new SiteImageValidationException("Skriv en alternativ billedtekst på højst 300 tegn uden linjeskift.");
        return alt;
    }

    public static async Task<MemoryStream> ValidateAndEncodeAsync(Stream input, CancellationToken ct)
    {
        // Restrict the decoder explicitly: never allow SVG, PDF, delegates or filename-based input.
        var header = new byte[12];
        var read = await input.ReadAsync(header, ct);
        input.Position = 0;
        var format = read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 })
            ? MagickFormat.Png
            : read >= 3 && header[0] == 255 && header[1] == 216 && header[2] == 255
                ? MagickFormat.Jpeg
                : read == 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8)
                    ? MagickFormat.WebP : MagickFormat.Unknown;
        if (format == MagickFormat.Unknown) throw InvalidImage();
        if (format is MagickFormat.Png or MagickFormat.WebP) RejectAnimationChunks(input, format);
        var settings = new MagickReadSettings { Format = format, FrameCount = 2 };
        try
        {
            // Ping checks dimensions and frame count without allocating decoded pixel buffers.
            using var frames = new MagickImageCollection();
            await frames.PingAsync(input, settings, ct);
            if (frames.Count != 1 || frames[0].Width > 8192 || frames[0].Height > 8192 ||
                (long)frames[0].Width * frames[0].Height > MaxPixels)
                throw new SiteImageValidationException("Vælg et stillbillede i JPEG-, PNG- eller WebP-format på højst 16 megapixel og 8192 pixels pr. side.");
            input.Position = 0;
            using var image = new MagickImage();
            await image.ReadAsync(input, new MagickReadSettings { Format = format, FrameCount = 1 }, ct);
            image.AutoOrient();
            // Re-encode rather than serving user-supplied bytes. Drop EXIF/GPS and other metadata.
            image.Strip();
            image.Quality = 90;
            var output = new MemoryStream();
            try
            {
                await image.WriteAsync(output, MagickFormat.WebP, ct);
                output.Position = 0;
                return output;
            }
            catch
            {
                output.Dispose();
                throw;
            }
        }
        catch (MagickException)
        {
            throw InvalidImage();
        }
    }

    private static SiteImageValidationException InvalidImage() =>
        new("Filen kunne ikke læses som et gyldigt JPEG-, PNG- eller WebP-billede.");

    private static void RejectAnimationChunks(Stream input, MagickFormat format)
    {
        // The PNG decoder can otherwise silently flatten APNG. Inspect container chunk types,
        // never byte-search compressed pixels (which can contain arbitrary matching strings).
        input.Position = format == MagickFormat.Png ? 8 : 12;
        Span<byte> header = stackalloc byte[8];
        while (input.Position + 8 <= input.Length)
        {
            input.ReadExactly(header);
            var png = format == MagickFormat.Png;
            var size = png ? BinaryPrimitives.ReadUInt32BigEndian(header[..4]) : BinaryPrimitives.ReadUInt32LittleEndian(header[4..]);
            var type = png ? header[4..] : header[..4];
            if (type.SequenceEqual("acTL"u8) || type.SequenceEqual("ANIM"u8) || type.SequenceEqual("ANMF"u8))
                throw new SiteImageValidationException("Animerede billeder understøttes ikke.");
            var skip = (long)size + (png ? 4 : size % 2);
            if (skip > input.Length - input.Position) throw InvalidImage();
            input.Position += skip;
            if (png && type.SequenceEqual("IEND"u8)) break;
        }
        input.Position = 0;
    }
}

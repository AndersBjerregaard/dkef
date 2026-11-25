using System.Text;
using Dkef.Services;
using ImageMagick;
using Xunit;

namespace Dkef.Tests;

public class SiteImageValidatorTests
{
    internal static byte[] ImageBytes(string format = "PNG", int width = 8, int height = 6, bool animated = false)
    {
        using var image = new MagickImage(MagickColors.Red, (uint)width, (uint)height);
        var exif = new ExifProfile();
        exif.SetValue(ExifTag.ImageDescription, "private-upload-metadata");
        image.SetProfile(exif);
        using var bytes = new MemoryStream();
        var outputFormat = format switch
        {
            "JPEG" => MagickFormat.Jpeg,
            "Webp" => MagickFormat.WebP,
            _ => MagickFormat.Png
        };
        if (animated)
        {
            if (outputFormat == MagickFormat.Png)
            {
                image.Write(bytes, outputFormat);
                var png = bytes.ToArray();
                // Animation control chunk before IDAT; rejected without invoking an APNG delegate.
                return [.. png[..33], 0, 0, 0, 8, (byte)'a', (byte)'c', (byte)'T', (byte)'L', 0, 0, 0, 2, 0, 0, 0, 0, 0, 0, 0, 0, .. png[33..]];
            }
            using var frames = new MagickImageCollection { image.Clone(), new MagickImage(MagickColors.Blue, (uint)width, (uint)height) };
            foreach (var frame in frames) frame.AnimationDelay = 10;
            frames.Write(bytes, outputFormat);
        }
        else image.Write(bytes, outputFormat);
        return bytes.ToArray();
    }

    [Theory]
    [InlineData("JPEG")]
    [InlineData("PNG")]
    [InlineData("Webp")]
    public async Task ValidImagesAreReencodedAndPrivateMetadataIsRemoved(string format)
    {
        using var input = new MemoryStream(ImageBytes(format));
        using var encoded = await SiteImageValidator.ValidateAndEncodeAsync(input, default);
        using var decoded = new MagickImage(encoded);
        Assert.Equal(MagickFormat.WebP, decoded.Format);
        Assert.Equal(8U, decoded.Width);
        Assert.Equal(6U, decoded.Height);
        Assert.Null(decoded.GetExifProfile());
    }

    [Theory]
    [InlineData("<svg xmlns='http://www.w3.org/2000/svg'><script>alert(1)</script></svg>")]
    [InlineData("not an image")]
    public async Task NonImagesAndSvgAreRejected(string data)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(data));
        await Assert.ThrowsAsync<SiteImageValidationException>(() => SiteImageValidator.ValidateAndEncodeAsync(stream, default));
    }

    [Theory]
    [InlineData(8193, 1)]
    [InlineData(4001, 4000)]
    public async Task ExcessiveDimensionsAreRejected(int width, int height)
    {
        using var stream = new MemoryStream(ImageBytes(width: width, height: height));
        await Assert.ThrowsAsync<SiteImageValidationException>(() => SiteImageValidator.ValidateAndEncodeAsync(stream, default));
    }

    [Theory]
    [InlineData("PNG")]
    [InlineData("Webp")]
    public async Task AnimatedImagesAreRejected(string format)
    {
        using var stream = new MemoryStream(ImageBytes(format, animated: true));
        await Assert.ThrowsAsync<SiteImageValidationException>(() => SiteImageValidator.ValidateAndEncodeAsync(stream, default));
    }

    [Fact]
    public async Task ExifOrientationIsAppliedBeforeMetadataIsRemoved()
    {
        using var image = new MagickImage(MagickColors.Red, 2, 3);
        var exif = new ExifProfile();
        exif.SetValue(ExifTag.Orientation, (ushort)6);
        image.SetProfile(exif);
        image.Orientation = Orientation.RightTop;
        using var input = new MemoryStream();
        image.Write(input, MagickFormat.Jpeg);
        input.Position = 0;
        using var encoded = await SiteImageValidator.ValidateAndEncodeAsync(input, default);
        using var result = new MagickImage(encoded);
        Assert.Equal(3U, result.Width);
        Assert.Equal(2U, result.Height);
        Assert.Null(result.GetExifProfile());
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("<b></b>")]
    [InlineData("to\nlinjer")]
    [InlineData("to\u2028linjer")]
    public void EmptyAndMultilineAlternativeDescriptionsAreRejected(string alt) =>
        Assert.Throws<SiteImageValidationException>(() => SiteImageValidator.ValidateAlt(alt));

    [Fact]
    public void DescriptionsAreSanitizedAndBounded()
    {
        Assert.Equal("Elmast & kabler", SiteImageValidator.ValidateAlt(" <b>Elmast</b> &amp; kabler "));
        Assert.Equal(new string('a', 300), SiteImageValidator.ValidateAlt(new string('a', 300)));
        Assert.Throws<SiteImageValidationException>(() => SiteImageValidator.ValidateAlt(new string('a', 301)));
    }
}

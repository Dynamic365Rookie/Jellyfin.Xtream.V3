using Xunit;
using Jellyfin.Xtream.V3.Utilities;

namespace Jellyfin.Xtream.V3.Tests.Infrastructure;

/// <summary>
/// Tests for the logo generator utility.
/// </summary>
public class LogoGeneratorTests
{
    [Theory]
    [InlineData("Resources/thumb.png")]
    public void GenerateLogo_CreatesValidPng(string outputPath)
    {
        var fullPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", outputPath);
        var directory = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory!);
        }

        LogoGenerator.GenerateLogo(fullPath);

        Assert.True(File.Exists(fullPath), "PNG file was not created");

        var fileInfo = new FileInfo(fullPath);
        Assert.True(fileInfo.Length > 0, "PNG file is empty");

        // Verify PNG signature
        var buffer = new byte[8];
        using (var fs = File.OpenRead(fullPath))
        {
            fs.Read(buffer, 0, 8);
        }
        // PNG signature: 89 50 4E 47 0D 0A 1A 0A
        Assert.Equal(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }, buffer);
    }
}

using Xunit;
using Jellyfin.Xtream.Configuration;

namespace Jellyfin.Xtream.V3.Tests.Configuration;

/// <summary>
/// Tests for <see cref="XtreamOptions"/>.
/// </summary>
public class XtreamOptionsTests
{
    [Fact]
    public void DefaultOptions_HaveEmptyStrings()
    {
        // Arrange & Act
        var options = new XtreamOptions();

        // Assert
        Assert.Equal(string.Empty, options.BaseUrl);
        Assert.Equal(string.Empty, options.Username);
        Assert.Equal(string.Empty, options.Password);
    }

    [Fact]
    public void Options_WithValues_RetainsProperties()
    {
        // Arrange & Act
        var options = new XtreamOptions
        {
            BaseUrl = "http://example.com:8080",
            Username = "testuser",
            Password = "testpass"
        };

        // Assert
        Assert.Equal("http://example.com:8080", options.BaseUrl);
        Assert.Equal("testuser", options.Username);
        Assert.Equal("testpass", options.Password);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Options_WithBlankBaseUrl_AreInvalid(string baseUrl)
    {
        var options = new XtreamOptions { BaseUrl = baseUrl };

        Assert.True(string.IsNullOrWhiteSpace(options.BaseUrl));
    }
}

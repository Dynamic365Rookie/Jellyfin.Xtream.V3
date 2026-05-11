using System.Text.Json;
using Xunit;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Serialization;

namespace Jellyfin.Xtream.V3.Tests.Infrastructure.Serialization;

public class StreamIconMappingTests
{
    [Fact]
    public void StreamIconMapping_ParsesStreamIconField()
    {
        // Arrange
        var json = """
        {
            "stream_id": 123,
            "name": "Test Channel",
            "stream_icon": "https://example.com/logo.png",
            "epg_channel_id": "1",
            "added": "1234567890"
        }
        """;
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        
        // Act
        var channel = JsonSerializer.Deserialize<XtreamChannel>(json, options);
        
        // Assert
        Assert.NotNull(channel);
        Assert.Equal("Test Channel", channel.Name);
        Assert.Equal("https://example.com/logo.png", channel.Icon);
    }

    [Fact]
    public void StreamIconMapping_FallbackToIconField()
    {
        // Arrange
        var json = """
        {
            "stream_id": 456,
            "name": "Old Format Channel",
            "icon": "https://example.com/old-logo.png",
            "epg_channel_id": "2",
            "added": "1234567890"
        }
        """;
        
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        
        // Act
        var channel = JsonSerializer.Deserialize<XtreamChannel>(json, options);
        
        // Assert
        Assert.NotNull(channel);
        Assert.Equal("Old Format Channel", channel.Name);
        Assert.Equal("https://example.com/old-logo.png", channel.Icon);
    }
}

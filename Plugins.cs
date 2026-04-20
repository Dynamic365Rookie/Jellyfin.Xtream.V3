using System;
using System;
using System.Collections.Generic;
using Jellyfin.Xtream.V3.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Xtream.V3;

public sealed class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public override string Name => "Jellyfin Xtream";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("5d774c35-8567-46d3-a950-9bb8227a0c5d");

    /// <inheritdoc />
    public override string Description => "Performance-optimized IPTV plugin for Xtream Codes API. Support for 25,000+ entities with advanced caching and memory management.";

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = Name,
                EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
            }
        };
    }
}

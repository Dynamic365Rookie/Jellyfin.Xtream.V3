using System;
using System;
using System.Collections.Generic;
using System.Reflection;
using Jellyfin.Xtream.V3.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Xtream.V3;

/// <summary>
/// The main plugin class.
/// </summary>
public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Plugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
        LogPluginInitialization();
    }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static Plugin? Instance { get; private set; }

    /// <inheritdoc />
    public override string Name => "Jellyfin Xtream";

    /// <inheritdoc />
    public override Guid Id => Guid.Parse("a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d");

    /// <inheritdoc />
    public override string Description => "IPTV plugin for Xtream Codes API";

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        return new[]
        {
            new PluginPageInfo
            {
                Name = this.Name,
                EmbeddedResourcePath = string.Format("{0}.Configuration.configPage.html", GetType().Namespace)
            }
        };
    }

    /// <summary>
    /// Logs plugin initialization details for debugging.
    /// </summary>
    private void LogPluginInitialization()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            var assemblyName = assembly.GetName().Name;

            System.Diagnostics.Debug.WriteLine($"[Jellyfin.Xtream] Plugin Initializing: {assemblyName} v{version}");
            System.Diagnostics.Debug.WriteLine($"[Jellyfin.Xtream] Assembly Location: {assembly.Location}");

            // Log loaded assemblies in the plugin directory
            var pluginPath = Path.GetDirectoryName(assembly.Location);
            if (!string.IsNullOrEmpty(pluginPath))
            {
                var dllFiles = Directory.GetFiles(pluginPath, "*.dll");
                System.Diagnostics.Debug.WriteLine($"[Jellyfin.Xtream] Plugin directory DLLs ({dllFiles.Length} total):");
                foreach (var dll in dllFiles.OrderBy(f => f))
                {
                    var fileInfo = new FileInfo(dll);
                    System.Diagnostics.Debug.WriteLine($"  - {fileInfo.Name} ({fileInfo.Length} bytes)");
                }
            }

            System.Diagnostics.Debug.WriteLine("[Jellyfin.Xtream] Plugin initialization complete ✓");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Jellyfin.Xtream] Error during plugin initialization logging: {ex.Message}");
        }
    }
}

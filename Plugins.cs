using MediaBrowser.Common.Plugins;

namespace Jellyfin.Xtream.V2;

public sealed class Plugin : BasePlugin
{
    public Plugin()
    {
    }

    public override string Name => "Xtream IPTV";
    
    public override string Description => "Plugin IPTV optimisé pour les services Xtream - Support jusqu'à 25,000+ entités";
}

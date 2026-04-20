using Jellyfin.Xtream.Configuration;

namespace Jellyfin.Xtream.Services.LiveTv;

public sealed class StreamUrlResolver
{
    private readonly XtreamOptions _options;

    public StreamUrlResolver(XtreamOptions options)
    {
        _options = options;
    }

    public string Resolve(int streamId)
    {
        return $"{_options.BaseUrl}/live/" +
               $"{_options.Username}/{_options.Password}/{streamId}.ts";
    }

}

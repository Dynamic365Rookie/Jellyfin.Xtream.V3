/* 
 * Ce service nécessite MediaBrowser.Controller qui n'est pas disponible sur NuGet public.
 * Il sera disponible quand le plugin sera compilé dans le contexte de Jellyfin.
 * Pour l'instant, ce fichier est commenté pour permettre la compilation.
 */

/*
using Jellyfin.Xtream.Api;
using Jellyfin.Xtream.Domain.Models;
using Jellyfin.Xtream.Infrastructure.Persistence;
using MediaBrowser.Controller.LiveTv;
using MediaBrowser.Model.LiveTv;
using MediaBrowser.Model.Dto;


namespace Jellyfin.Xtream.Services.LiveTv;

public sealed class XtreamLiveTvService : ILiveTvService
{
    private readonly XtreamApiClient _api;
    private readonly StreamUrlResolver _resolver;
    private readonly IXtreamRepository<XtreamChannel> _channels;

    public XtreamLiveTvService(
        XtreamApiClient api,
        StreamUrlResolver resolver,
        IXtreamRepository<XtreamChannel> channels)
    {
        _api = api;
        _resolver = resolver;
        _channels = channels;
    }

    public async Task<IEnumerable<ChannelInfo>> GetChannelsAsync(
        CancellationToken cancellationToken)
    {
        // À remplacer par appel Xtream réel
        var demoChannels = new[]
        {
            new XtreamChannel { Id = 1, Name = "Xtream Demo 1", StreamId = 1001 },
            new XtreamChannel { Id = 2, Name = "Xtream Demo 2", StreamId = 1002 }
        };

        foreach (var channel in demoChannels)
            _channels.Upsert(channel);

        return demoChannels.Select(c => new ChannelInfo
        {
            Id = c.Id.ToString(),
            Name = c.Name,
            ChannelType = ChannelType.TV
        });
    }

    public Task<MediaSourceInfo> GetChannelStreamAsync(
        string channelId,
        CancellationToken cancellationToken)
    {
        var streamId = int.Parse(channelId);

        var url = _resolver.Resolve(streamId);

        return Task.FromResult(new MediaSourceInfo
        {
            Path = url,
            Protocol = MediaProtocol.Http,
            IsRemote = true
        });
    }

    #region Not implemented yet (required by interface)

    public Task<IEnumerable<RecordingInfo>> GetRecordingsAsync(CancellationToken cancellationToken)
        => Task.FromResult(Enumerable.Empty<RecordingInfo>());

    public Task<IEnumerable<TimerInfo>> GetTimersAsync(CancellationToken cancellationToken)
        => Task.FromResult(Enumerable.Empty<TimerInfo>());

    public Task CancelTimerAsync(string timerId, CancellationToken cancellationToken)
        => Task.CompletedTask;

    public Task CreateTimerAsync(TimerInfo info, CancellationToken cancellationToken)
        => Task.CompletedTask;

    #endregion
}
*/

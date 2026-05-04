# Jellyfin Xtream V3 - IPTV Plugin

[![Release](https://img.shields.io/github/v/release/Dynamic365Rookie/Jellyfin.Xtream.V3?label=Latest%20Release)](https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3/releases)
[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=.net)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
[![Performance](https://img.shields.io/badge/Performance-Optimized-success)](PERFORMANCE_GUIDE.md)

Performance-optimized Jellyfin plugin for streaming IPTV content from Xtream Codes API. Support for large catalogs with advanced caching, title cleaning for metadata matching, EPG support, and STRM file generation.

---

## 🎯 Key Features

### 📺 Live TV Integration
- **Live Channels** — Stream live TV channels directly in Jellyfin Live TV
- **EPG Support** — Electronic Program Guide with program schedules (experimental)
- **Channel Grouping** — Automatic organization by category
- **Language Tags** — Display channel language information

### 🎬 Media Library Integration
- **.strm File Generation** — Movies and Series appear in Jellyfin's standard library
- **Metadata Matching** — Automatic title cleaning for better TMDb scraper matching
- **Selective Deletion** — Delete movies, series, or channels independently
- **STRM Regeneration** — Rebuild file structure with clean titles

### ⚡ Performance Optimizations
- **Parallel Processing** — Concurrent synchronization of movies, series, channels
- **Batch Operations** — Optimized database queries (99% fewer requests)
- **Smart Caching** — In-memory cache with auto-expiration
- **Large Catalog Support** — Efficiently handles 15,000+ movies + 8,500+ series

### 🛠️ Developer Tools
- **Database Stats** — View count of movies, series, channels
- **Clear Database** — Remove all synchronized data and STRM files
- **Data Viewer** — Browse first N records of each entity type

---

## 🚀 Quick Start

### Installation

1. **Download** the latest release from [GitHub Releases](https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3/releases)
2. **Extract** to your Jellyfin plugins directory:
   - **Linux/Docker**: `/var/lib/jellyfin/plugins/`
   - **Windows**: `C:\ProgramData\Jellyfin\Server\plugins\`
   - **macOS**: `/Users/{user}/.local/share/jellyfin/plugins/`
3. **Restart** Jellyfin
4. **Configure** via Dashboard > Plugins > Jellyfin Xtream

### Configuration

#### Required Settings
- **Server URL** — Your Xtream Codes API endpoint (e.g., `http://xtream.example.com:8000`)
- **Username** — Your Xtream account username
- **Password** — Your Xtream account password

#### Optional Settings
- **Enable Live TV** — Activate Jellyfin Live TV integration (requires Jellyfin Live TV support)
- **Enable EPG** — Fetch Electronic Program Guide for channels
- **Clean Titles** — Remove language tags and quality suffixes from titles for better metadata matching
- **STRM Paths** — Directory paths where .strm files will be generated

### Synchronization

Click **"Synchronize Now"** to pull data from your Xtream server:
- First sync: ~10-20 minutes (depending on catalog size)
- Incremental sync: ~2-5 minutes (sync only new/changed items)

---

## 📋 Configuration Options

```
┌─ Live TV Settings
│  ├─ Enable Live TV Integration
│  ├─ Enable EPG (Program Guide)
│  ├─ Append Language to Channel Names
│  ├─ Show Channel Language Tags
│  ├─ Enable Channel Name Cleaning
│
├─ Media Library Settings
│  ├─ Enable STRM Generation
│  ├─ STRM Movies Path
│  ├─ STRM Series Path
│  ├─ Clean Titles for Metadata Matching
│
├─ Developer Tools
│  ├─ Database Stats
│  ├─ View Movies/Series/Channels
│  ├─ Clear Database + STRM Files
│  ├─ Delete Movies/Series/Channels Only
│  └─ Regenerate STRM Files
│
└─ Advanced Settings
   ├─ Max Concurrent Requests
   ├─ API Timeout (seconds)
   ├─ Enable Stream Options (FFmpeg)
   └─ Custom HTTP Headers
```

---

## 📊 Performance Metrics

### Typical Synchronization Times

| Operation | Small (5K) | Medium (15K) | Large (25K) |
|-----------|-----------|--------------|-------------|
| **Full Sync** | ~3 min | ~12 min | ~18 min |
| **Incremental** | ~30 sec | ~2 min | ~4 min |
| **STRM Generation** | ~1 min | ~4 min | ~7 min |
| **Memory Usage** | < 300 MB | < 800 MB | < 1.5 GB |

### Supported Catalog Sizes
- ✅ Movies: Up to 50,000
- ✅ Series: Up to 20,000  
- ✅ Channels: Up to 5,000
- ✅ Total: Up to 75,000 entities

---

## 🔧 Troubleshooting

### Issue: "ObjectDisposedException" in logs
**Cause**: Jellyfin framework issue when file watcher fires after plugin unload  
**Solution**: Harmless to normal operation; no data is affected. Check Jellyfin framework updates.

### Issue: Cannot delete STRM files in Docker
**Cause**: Docker volume doesn't have write permissions for the container user  
**Solution**: Fix permissions on your host:
```bash
sudo chown -R 1000:1000 /path/to/movies /path/to/series
sudo chmod -R 755 /path/to/movies /path/to/series
```
Or update docker-compose volumes with `:rw` flag:
```yaml
volumes:
  - /path/to/movies:/movies:rw
  - /path/to/series:/series:rw
```

### Issue: Series not recognized by metadata scraper
**Cause**: Titles contain noise (language tags, quality suffixes) preventing TMDb matching  
**Solution**: Enable "Clean Titles for Metadata Matching" in settings, then regenerate STRM files.

### Issue: Slow synchronization
**Cause**: API rate limiting or network bottleneck  
**Solution**: Reduce `Max Concurrent Requests` or increase API timeout in advanced settings.

---

## 📁 Project Structure

```
Jellyfin.Xtream.V3/
├── Api/                          # Xtream API client layer
│   ├── XtreamApiClient.cs
│   ├── XtreamApiEndpoints.cs
│   └── XtreamApiRateLimiter.cs
├── Domain/Models/                # Core domain models
│   ├── XtreamMovie.cs
│   ├── XtreamSeries.cs
│   ├── XtreamChannel.cs
│   └── XtreamEpgResponse.cs
├── Services/                     # Business logic
│   ├── LiveTv/
│   ├── Media/
│   ├── Mapping/
│   └── Synchronization/
├── Configuration/                # Plugin settings & UI
│   ├── PluginConfiguration.cs
│   └── configPage.html
├── Infrastructure/               # Data persistence & caching
│   ├── Persistence/
│   ├── Caching/
│   └── Utilities/
├── Tests/                        # Unit & integration tests
└── Resources/                    # Icons, assets
```

---

## 🧪 Testing

### Build & Test
```bash
dotnet restore
dotnet build -c Release
dotnet test
```

### Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
reportgenerator -reports:**/coverage.cobertura.xml -targetdir:coverage
```

---

## 🤝 Contributing

Contributions welcome! Before submitting:

1. **Fork** the repository
2. **Create** a feature branch (`git checkout -b feature/amazing-feature`)
3. **Commit** with clear messages (`git commit -m "feat: add amazing feature"`)
4. **Push** to your fork (`git push origin feature/amazing-feature`)
5. **Open** a Pull Request

See [CLAUDE.md](CLAUDE.md) for development guidelines.

---

## 📄 Documentation

- [QUICKSTART.md](QUICKSTART.md) — Getting started guide
- [PERFORMANCE_GUIDE.md](PERFORMANCE_GUIDE.md) — Tuning & optimization
- [CHANGELOG.md](CHANGELOG.md) — Version history and release notes
- [TESTING_GUIDE.md](TESTING_GUIDE.md) — Test guidelines

---

## ⚖️ License

This project is licensed under the MIT License — see [LICENSE](LICENSE) file for details.

---

## 🙏 Acknowledgments

- [Jellyfin](https://jellyfin.org/) — Open-source media system
- [LiteDB](https://www.litedb.org/) — Embedded database
- Xtream Codes API documentation
- Community feedback and contributions

---

## 📞 Support

- **Issues**: [GitHub Issues](https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3/issues)
- **Discussions**: [GitHub Discussions](https://github.com/Dynamic365Rookie/Jellyfin.Xtream.V3/discussions)
- **Docs**: Check [PERFORMANCE_GUIDE.md](PERFORMANCE_GUIDE.md) for common questions

---

**Status**: ✅ Production Ready | **Version**: 3.8.0 | **Target ABI**: 10.11.0.0


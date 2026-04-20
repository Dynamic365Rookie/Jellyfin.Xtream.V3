# Plugin Loading Troubleshooting Guide

## Problem Summary

**Symptom**: Plugin assembly loads but plugin itself is not recognized by Jellyfin

```
Logs show:
? [INF] Loaded assembly "Jellyfin.Xtream.V3, Version=1.0.0.0..."
? NO "Loaded plugin: Jellyfin Xtream" message
```

---

## Diagnostic Steps

### Step 1: Verify Plugin Class

**Requirements for Jellyfin Plugin:**
- Must inherit from `BasePlugin<TConfiguration>`
- Must be `public` (not sealed in some Jellyfin versions)
- Must have correct constructor signature
- Must be in correct namespace matching assembly

**Current Status:**
- ? Inherits from `BasePlugin<PluginConfiguration>`
- ? Public class (changed from sealed)
- ? Constructor with `IApplicationPaths` and `IXmlSerializer`
- ? Namespace `Jellyfin.Xtream.V3` matches assembly

### Step 2: Check Assembly Compatibility

**MediaBrowser.Common Version Issues:**
- Current: 4.9.1.90 (Jellyfin 10.9.x era)
- Jellyfin Server: 10.11.8.0
- **Potential incompatibility!**

**Options:**
1. Find correct MediaBrowser.Common for Jellyfin 10.11
2. Use different package (Jellyfin.Controller, Jellyfin.Model)
3. Reference Jellyfin assemblies directly

### Step 3: Verify meta.json

**Current meta.json:**
```json
{
  "guid": "a1b2c3d4-5e6f-7a8b-9c0d-1e2f3a4b5c6d",
  "name": "Jellyfin Xtream",
  "version": "3.1.4",
  "targetAbi": "10.11.0.0"
}
```

**Status**: ? Format correct

### Step 4: Check for Silent Exceptions

Jellyfin may silently fail to load plugin if:
- Constructor throws exception
- Configuration class invalid
- Missing dependencies

**Action**: Add try-catch logging in constructor

---

## Attempted Fixes (v3.1.0 - v3.1.4)

1. ? Added configuration UI (configPage.html)
2. ? Created PluginConfiguration class
3. ? Fixed namespace (V2 ? V3)
4. ? Changed GUID (avoid conflicts)
5. ? Added meta.json manifest
6. ? Fixed meta.json version format
7. ? Removed sealed modifier
8. ?? MediaBrowser.Common version still old

---

## Next Steps to Try

### Option A: Minimal Plugin (Current Approach v3.1.5)
- Remove all complexity
- Simplest possible plugin
- Test if it loads

### Option B: Match Working Plugin Exactly
- Study Kevinjil's Jellyfin.Xtream source
- Use exact same packages
- Use exact same structure

### Option C: Different Package Strategy
- Try without MediaBrowser.Common
- Reference Jellyfin assemblies from installation
- Use only .NET Core packages

---

## Test Plan for v3.1.5

1. Build minimal plugin
2. Install in Jellyfin
3. Check logs for "Loaded plugin: Jellyfin Xtream"
4. If still fails: Enable debug logging in Jellyfin
5. Check for exception messages

---

## Encoding Problem (Separate Issue)

**Symptom**: Markdown files show `?` and `??` instead of special characters

**Cause**: Files not in UTF-8 or BOM issue

**Solution**: 
- Re-save all .md files as UTF-8 without BOM
- Or use ASCII-only characters
- Remove emojis and special chars from release notes

---

## Status

- Current Version Testing: v3.1.4
- Next Version: v3.1.5 (minimal plugin)
- Encoding Fix: Pending (requires file recreation)

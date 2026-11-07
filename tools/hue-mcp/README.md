# Hue MCP Server

An MCP (Model Context Protocol) server that exposes Philips Hue light control via stdio or loopback HTTP(S) transport. Works with Claude Desktop, VS Code Copilot, and any MCP-compatible client.

## Features

- **Light Control**: Set individual lights or entire rooms — brightness, color (hex/RGB/named), color temperature
- **Scene Management**: Create, activate, edit, and delete scenes (compatible with Hue Companion app scenes)
- **Animated Scenes**: Create keyframe-based animations with per-light tracks
- **Event Effects**: Lightning flash, sparkle, candle flicker effects
- **Multi-Bridge**: Supports multiple Hue bridges simultaneously
- **Fuzzy Matching**: Find lights and rooms by partial name match

## Tools

| Tool | Description |
|------|-------------|
| `hue_list_rooms` | List all rooms/zones with light states |
| `hue_get_light` | Get a specific light's state |
| `hue_set_light` | Control a light (on/off, brightness, color, temperature) |
| `hue_set_room` | Control all lights in a room |
| `hue_turn_off_all` | Turn off everything |
| `hue_list_scenes` | List saved scenes |
| `hue_activate_scene` | Activate a scene in a room |
| `hue_create_scene` | Create a static scene |
| `hue_delete_scene` | Delete a scene |
| `hue_create_animated_scene` | Create an animated scene with keyframes |
| `hue_play_animation` | Start an animated scene |
| `hue_stop_animation` | Stop animations |
| `hue_edit_scene` | Edit scene keyframes/effects |
| `hue_list_event_presets` | List available effect presets |
| `hue_list_bridges` | List configured bridges |

## Setup

### Prerequisites

- ASP.NET Core 8 Runtime for framework-dependent builds (the project uses `Microsoft.NET.Sdk.Web`), or a self-contained publish
- At least one Philips Hue bridge configured

### Bridge Configuration

The MCP server reads bridge credentials from `%LOCALAPPDATA%/HueCompanion/bridges.json`:

```json
[
  {
    "BridgeId": "001788FFFE123456",
    "IpAddress": "192.168.1.50",
    "AppKey": "your-app-key-here",
    "FriendlyName": "Living Room Bridge"
  }
]
```

The normal app configuration is `%LOCALAPPDATA%/HueCompanion/settings.json`; the server reads it when no valid nonempty `bridges.json` list is loaded. No current WinUI writer for `bridges.json` exists. A successful bridge-list load returns before reading other saved settings. Credentials in both files are plaintext.

The host checks `McpEnabled` before serving tools. When using the settings fallback, enable MCP in the app’s Settings page. The app can manage the HTTP(S) host as a child process, or a client/user can launch the executable independently.

### Building

```bash
# Framework-dependent (requires .NET 8 runtime)
dotnet build src/HueCompanion.Mcp -c Release

# Self-contained single file (no runtime needed)
dotnet publish src/HueCompanion.Mcp -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o dist/mcp
```

### Easiest Setup

Use the **copy button** in Hue Companion's Settings → MCP Server section — it generates the config with the correct executable path for your system.

For step-by-step instructions with screenshots, see the [MCP Setup Guide](https://hue-companion.apps.drayne.xyz/mcp-setup).

### Claude Desktop Configuration

Add to `%APPDATA%/Claude/claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "hue-companion": {
      "command": "C:/path/to/HueCompanion.Mcp.exe",
      "args": ["--stdio"]
    }
  }
}
```

### Claude Code Configuration

Add to `.mcp.json` in your project root:

```json
{
  "mcpServers": {
    "hue-companion": {
      "command": "C:/path/to/HueCompanion.Mcp.exe",
      "args": ["--stdio"]
    }
  }
}
```

### VS Code / GitHub Copilot Configuration

Start the MCP HTTP(S) host through Hue Companion Settings or independently. Default HTTPS requires Kestrel certificate configuration. The endpoint is `https://localhost:5680` at the root: `app.MapMcp()` has no prefix, despite the startup hint that appends `/mcp`. Use `--http` for `http://localhost:5680`, or `--port` to change the port.

Add to `.vscode/mcp.json` in your workspace:

```json
{
  "servers": {
    "hue-companion": {
      "type": "http",
      "url": "https://localhost:5680"
    }
  }
}
```

This example uses the VS Code `servers` format, which differs from the portable `mcpServers` format. See the [official VS Code MCP configuration guidance](https://code.visualstudio.com/docs/agent-customization/mcp-servers) for the supported configuration destinations and guided setup.

## Example Usage

Once configured, you can ask your AI assistant things like:

- "Turn the bedroom lights to a warm orange at 50% brightness"
- "What lights are on right now?"
- "Create a sunset animation that goes from warm white to deep red over 30 seconds"
- "Activate the Movie Night scene in the living room"
- "Turn off all the lights"

## Color Formats

The server accepts colors in several formats:

- **Hex**: `#FF0000`, `FF0000`
- **RGB**: `rgb(255, 0, 0)`
- **Named**: `red`, `blue`, `warm white`, `coral`, `lavender`, etc.

## Scene Compatibility

Scenes are stored as JSON in `%LOCALAPPDATA%/HueCompanion/Scenes/` using the same Core scene model as the desktop app. The editor imports native effects into its editable representation; verify round trips for effect-specific fields. In `AnimationEngine`, `Priority` does not arbitrate overlapping writes, and native-effect playback does not forward saved effect speed/brightness.

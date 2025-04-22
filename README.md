# Session Logger

A Unity package for tracking and analyzing session data in your VR/AR applications.

## Features

- Automatic tracking of session start and end times
- Custom event logging during gameplay
- Completion percentage calculation for objectives
- Performance metrics (FPS) monitoring
- Local saving and remote server uploading
- Separate Editor and Build configurations
- Safe application closing with data sending

## Installation

### Using Unity Package Manager

1. Open your Unity project
2. Go to Window > Package Manager
3. Click the "+" button and select "Add package from disk..."
4. Navigate to the SessionLogger folder and select the `package.json` file
5. Click "Add"

### Manual Installation

1. Copy the entire SessionLogger folder into your Unity project's "Packages" directory
2. Restart Unity or refresh the package list

## Quick Setup

1. Create a configuration asset: Right-click > Create > Session Logger > Setup
2. Place the asset in `Assets/Resources/SessionLoggerSetup.asset`
3. Add an empty GameObject to your initial scene and add the SessionLogger component
4. Assign the configuration asset to the "Config" field of the component

## Basic Usage

```csharp
// Import the namespace
using Inimart.SessionLogger;

// Log an event
SessionLogger.Instance.LogEvent("EventName");

// For frequently used events, use the ready-made component
// Add SessionLoggerLogEventSender to a GameObject and configure it in the Inspector
```

## Complete Documentation

For detailed instructions, advanced examples, and troubleshooting information, check out:

- [User Guide](Documentation~/SessionLogger-Guide.md)
- [API Reference](Documentation~/API-Reference.md)

## Requirements

- Unity 2021.3 or newer
- Compatible with all platforms, optimized for Oculus Quest
- .NET Standard 2.0

## Contributing

To contribute to the project, contact the authors or open an issue on GitHub.

## License

This package is distributed under the MIT License. See [LICENSE.md](LICENSE.md) for details. 
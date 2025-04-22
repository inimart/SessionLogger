# Session Logger - Complete Guide

## Overview

Hey there! This guide will walk you through Session Logger, a handy Unity package for tracking player sessions in your games and apps. This tool lets you collect all sorts of useful gameplay data, performance metrics, and send everything to a server for deeper analysis.

With Session Logger, you can:
- See how players are interacting with your game
- Track when important actions are completed
- Spot performance issues before they become problems
- Analyze usage patterns to improve your game design

## Requirements

- Unity 2021.3 or newer
- Works with all Unity-supported platforms (but it's especially great for VR apps on Oculus Quest)
- Needs at least .NET Standard 2.0

## Installation

### Using Unity Package Manager (Recommended)

1. Open Unity and your project
2. Go to Window > Package Manager
3. Click the "+" button and select "Add package from disk..."
4. Navigate to the SessionLogger folder and select the `package.json` file
5. Click "Open" to add the package to your project

### Manual Installation

1. Copy the SessionLogger folder into your Unity project's "Packages" directory
2. Restart Unity or refresh package references (through Package Manager > Reset packages to defaults)

## Getting Started

### 1. Create a Config Asset

1. In the Project window, right-click and select Create > Session Logger > Setup
2. Rename the created asset to "SessionLoggerSetup"
3. Place the asset in a Resources folder in your project (e.g., `Assets/Resources/SessionLoggerSetup.asset`)

### 2. Configure Your SessionLoggerSetup Asset

Select the asset you created and in the Inspector panel, configure:

- **Editor Settings**: how logging works in the Unity Editor
  - `saveLocalJson`: save logs as JSON files locally
  - `sendToServer`: send logs to your remote server

- **Build Settings**: how logging works in your built game
  - `saveLocalJson`: save logs as JSON files locally
  - `sendToServer`: send logs to your remote server

- **Server Configuration**:
  - `serverUrl`: URL of the server that will receive your logs
  - `serverApiKey`: API key for authentication (optional)
  - `serverApiKeyHeader`: HTTP header name for the API key (default: "X-API-Key")

- **Session Actions**: define all the actions you want to track in your game
  - Add meaningful names to actions (e.g., "TutorialComplete", "LevelStart", "ItemCollected")

### 3. Add the SessionLogger Component

1. Create an empty GameObject in your initial scene
2. Name it "SessionLogger"
3. Add the SessionLogger component via Add Component
4. Set the "Config" property to the SessionLoggerSetup asset you created
5. Make sure DontDestroyOnLoad is active (it is by default)

## How to Use It

### Tracking Events

To log a specific event during gameplay, just do this:

```csharp
using Inimart.SessionLogger;

// From any script in your game
void OnLevelComplete()
{
    // Make sure "LevelComplete" is defined in the ActionNames array in SessionLoggerSetup
    SessionLogger.Instance.LogEvent("LevelComplete");
}
```

### Using the SessionLoggerLogEventSender Component

The `SessionLoggerLogEventSender` component is a super easy way to track events through the Inspector:

1. Add the `SessionLoggerLogEventSender` component to any GameObject
2. Select the action you want to track from the "Action To Log" dropdown in the Inspector
3. Optionally, check "Fire On Enable" to send the event automatically when the GameObject is enabled
4. To trigger the event manually, call the `SendConfiguredLogEvent()` method from another script or a UnityEvent

```csharp
// Example of how to call the method from code
public SessionLoggerLogEventSender eventSender;

void TriggerLogEvent()
{
    eventSender.SendConfiguredLogEvent();
}
```

## What's in Your Logs

The logs you generate contain:

- App info (name, version)
- Session start time and duration
- Performance data (highest and lowest FPS)
- List of completed actions with counts
- Completion percentage
- Error and warning logs

## Testing Your Setup

To test the logging system:

1. Make sure `saveLocalJson` is enabled in the Editor settings
2. Run your app in the Editor
3. Interact with your game to generate some events
4. Check the console for messages about log saving
5. The JSON files will be saved in the `Application.persistentDataPath` directory

## Advanced Customization

### Changing the Quit Timeout

By default, the system waits up to 5 seconds to finish sending logs before quitting the app. You can change this:

```csharp
// Extend the timeout to 10 seconds
SessionLogger.Instance.maxQuitWaitTime = 10f;
```

### Saving Logs Manually

To force save and send logs at any time:

```csharp
// Save and send
SessionLogger.Instance.SaveAndSend();

// Save with a callback
SessionLogger.Instance.SaveAndSendWithCallback((success) => {
    Debug.Log($"Log sent successfully: {success}");
});
```

## Troubleshooting

### Logs Aren't Being Sent to the Server

- Double-check your server URL is correct
- Make sure `sendToServer` is enabled in your configuration
- Check that your device is connected to the internet
- Look for error logs in the console for any connection issues

### Events Not Being Recorded

- Make sure the event name exists in the `ActionNames` array in your SessionLoggerSetup asset
- Check that SessionLogger.Instance isn't null when you call LogEvent()
- Verify the system wasn't disabled due to initialization errors

## VR Considerations

When using the package in VR applications:

- Try not to log too many events in a short time to avoid performance hits
- The FPS data recorded can be super useful for optimizing VR experiences
- The system handles app closure properly even in VR environments, making sure logs get sent before quitting 
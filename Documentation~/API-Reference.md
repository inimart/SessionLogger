# Session Logger API Reference

Hey there! This is a quick reference guide to the main classes and methods in the Session Logger package. Use this when you need to remember exactly how a method works or what parameters it needs.

## Main Classes

### `SessionLogger`

The heart of the package - this singleton handles all logging and session management.

```csharp
using Inimart.SessionLogger;

// Access the instance anywhere
SessionLogger.Instance.LogEvent("YourEventName");
```

#### Important Properties

| Property          | Type                 | Description                                                         |
| ----------------- | -------------------- | ------------------------------------------------------------------- |
| `Instance`        | `SessionLogger`      | Static singleton instance for easy access                           |
| `config`          | `SessionLoggerSetup` | Reference to your configuration asset                               |
| `maxQuitWaitTime` | `float`              | How many seconds to wait for log sending when quitting (default: 5) |

#### Key Methods

**LogEvent**

```csharp
void LogEvent(string eventName)
```

Logs a named event and increases its counter. The event name should be defined in your SessionLoggerSetup asset.

**SaveAndSend**

```csharp
void SaveAndSend()
```

Immediately saves the session log to disk and/or sends it to the server (based on your config settings).

**SaveAndSendWithCallback**

```csharp
void SaveAndSendWithCallback(Action<bool> onComplete)
```

Same as SaveAndSend but with a callback to tell you when it's done and if it succeeded.

### `SessionLoggerSetup`

ScriptableObject that holds all your configuration settings.

```csharp
// Create via menu: Create > Session Logger > Setup
```

#### Properties

| Property             | Type         | Description                                 |
| -------------------- | ------------ | ------------------------------------------- |
| `editorConfig`       | `ModeConfig` | Settings for when running in Unity Editor   |
| `buildConfig`        | `ModeConfig` | Settings for when running in a build        |
| `serverUrl`          | `string`     | URL for the server where logs are sent      |
| `serverApiKey`       | `string`     | Optional API key for server authentication  |
| `serverApiKeyHeader` | `string`     | HTTP header name for the API key            |
| `ActionNames`        | `string[]`   | Array of all action names you want to track |

### `SessionLoggerLogEventSender`

Helper component for sending log events from the Inspector or through UnityEvents.

```csharp
// Typically used through Inspector, but can be referenced in code
public SessionLoggerEventSender eventSender;
eventSender.SendConfiguredLogEvent();
```

#### Properties

| Property                 | Type                 | Description                                          |
| ------------------------ | -------------------- | ---------------------------------------------------- |
| `actionToLog`            | `string`             | The name of the event to log                         |
| `optionalConfigOverride` | `SessionLoggerSetup` | Optional different config to use                     |
| `FireOnEnable`           | `bool`               | Whether to automatically fire the event when enabled |

#### Key Methods

**SendConfiguredLogEvent**

```csharp
void SendConfiguredLogEvent()
```

Logs the event configured in the Inspector.

## Common Workflows

### Basic Logging Setup

```csharp
// 1. Make sure you have a SessionLoggerSetup asset in Resources
// 2. Add a SessionLogger component to a GameObject in your first scene
// 3. Log events from any script:

using Inimart.SessionLogger;

public class YourGameplayScript : MonoBehaviour
{
    void OnLevelComplete()
    {
        SessionLogger.Instance.LogEvent("LevelComplete");
    }
}
```

### Inspector-Based Event Logging

```csharp
// 1. Add a SessionLoggerLogEventSender component to any GameObject
// 2. Select the event name from the dropdown in the Inspector
// 3. Use UnityEvents to trigger it - great for UI buttons!

// Button (On Click) → YourGameObject → SessionLoggerLogEventSender.SendConfiguredLogEvent()
```

### Handling Quit Properly

The package automatically handles application quit by waiting for logs to send, but you can customize it:

```csharp
// Extend the wait time for slower connections:
SessionLogger.Instance.maxQuitWaitTime = 10f; // 10 seconds
```

## Tips & Tricks

- If using WebGL, remember that file saving works differently - prioritize the server upload feature
- Keep the logger in your persistent scene so it doesn't get destroyed between scene loads
- Check Unity's console for helpful debug messages if anything goes wrong! 



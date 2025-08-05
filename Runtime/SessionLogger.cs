using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

namespace Inimart.SessionLogger
{
    // Serializable struct for action counts (using string for action name)
    [Serializable]
    public struct SerializableActionCount
    {
        public string actionName;
        public int count;
    }

    public class SessionLogger : MonoBehaviour
    {
        public static SessionLogger Instance { get; private set; }

        [Header("Configuration")]
        [Tooltip("Assign the SessionLoggerSetup ScriptableObject here.")]
        public SessionLoggerSetup config; // Reference to the configuration asset

        // --- Private session state variables ---
        private string sessionStartTime;
        private float sessionStartTimestamp;
        private float sessionDuration;
        private string appName;
        private string appVersion;
        private string bundleVersionCode;
        private float highestAvgFps = 0f;
        private float lowestAvgFps = float.MaxValue;
        private FpsData highFpsData = null;
        private FpsData lowFpsData = null;
        private List<LogEntry> logEntries = new();
        private HashSet<string> uniqueLogs = new();
        // Dictionary uses string keys now
        private Dictionary<string, int> sessionActionCounts = new();
        // Dictionary for custom events
        private Dictionary<string, string> customEvents = new();
        private string fileTimestamp;
        // Cached config for the current mode
        private SessionLoggerSetup.ModeConfig currentModeConfig;
        private string currentServerUrl;
        private Coroutine periodicSaveCoroutine;

        // Shutdown handling variables
        private bool isQuitting = false;
        private bool logSendComplete = false;
        private float quitStartTime = 0f;
        [Tooltip("Maximum seconds to wait for log sending before forcing application to quit")]
        public float maxQuitWaitTime = 5f; // Maximum 5 seconds wait by default

        private string GetSessionFileName()
        {
            // Format: DD_MM_YY_HH_MM.json
            DateTime sessionStart = DateTime.Parse(sessionStartTime);
            return sessionStart.ToString("dd_MM_yy_HH_mm") + ".json";
        }
        
        private string GetLogFilePath()
        {
            // Ensure persistentDataPath is used for cross-platform compatibility
            return Path.Combine(Application.persistentDataPath, GetSessionFileName());
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            // Register the quit handler
            Application.wantsToQuit += HandleApplicationWantsToQuit;

            // --- Load Configuration ---
            if (config == null)
            {
                config = Resources.Load<SessionLoggerSetup>("SessionLoggerSetup");
                if (config == null)
                {
                    Debug.LogError("SessionLoggerSetup configuration asset not assigned and not found in Resources/SessionLoggerSetup! Logging will be disabled.");
                    enabled = false;
                    return;
                }
                else
                {
                    Debug.Log("SessionLoggerSetup loaded from Resources.");
                }
            }

    #if UNITY_EDITOR
            currentModeConfig = config.editorConfig;
            Debug.Log("SessionLogger running in EDITOR mode.");
    #else
            currentModeConfig = config.buildConfig;
            Debug.Log("SessionLogger running in BUILD mode.");
    #endif
            currentServerUrl = config.serverUrl;
            Debug.Log($"Config - Save Local: {currentModeConfig.saveLocalJson}, Send Server: {currentModeConfig.sendToServer}, Server URL: {currentServerUrl}");

            // --- Initialize ---
            fileTimestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            Application.logMessageReceivedThreaded += HandleLog;
            InitializeSession();
            StartCoroutine(FpsSamplingCoroutine());

            if (currentModeConfig.sendToServer)
            {
                CheckForUnsentLogs();
            }
            
            // Start periodic saving if configured
            if (config.UpdateLogTime > 0)
            {
                periodicSaveCoroutine = StartCoroutine(PeriodicSaveCoroutine());
            }
        }

        private void OnDestroy()
        {
            // Unregister the quit handler to prevent memory leaks
            Application.wantsToQuit -= HandleApplicationWantsToQuit;
            
            // Unregister log handler
            Application.logMessageReceivedThreaded -= HandleLog;
            
            // Stop periodic save coroutine
            if (periodicSaveCoroutine != null)
            {
                StopCoroutine(periodicSaveCoroutine);
            }
        }

        private bool HandleApplicationWantsToQuit()
        {
            // If we're already in the process of quitting and handled it, allow the quit
            if (isQuitting)
            {
                // If sending is complete or timeout expired, allow quitting
                float timeWaited = Time.realtimeSinceStartup - quitStartTime;
                
                if (logSendComplete || timeWaited >= maxQuitWaitTime)
                {
                    Debug.Log($"SessionLogger: Allowing application to quit. Log sending " + 
                            (logSendComplete ? "completed successfully." : $"timed out after {timeWaited:F1} seconds."));
                    return true;
                }
                
                // Still waiting for log send to complete and within timeout, prevent quitting
                return false;
            }
            
            // First time quitting is attempted
            isQuitting = true;
            quitStartTime = Time.realtimeSinceStartup;
            
            // If sending is enabled, start the process and block quitting temporarily
            if (enabled && currentModeConfig.sendToServer)
            {
                Debug.Log("SessionLogger: Application is quitting. Sending logs before exit...");
                SaveAndSendWithCallback(OnLogSendComplete);
                
                // Start monitoring the quit process
                StartCoroutine(MonitorQuitProcess());
                
                // Prevent immediate quit to give time for the log to be sent
                return false;
            }
            
            // If sending is not enabled, just save logs locally if configured
            if (enabled && currentModeConfig.saveLocalJson)
            {
                SaveAndSend();
            }
            
            // Allow quitting immediately if sending is not enabled
            return true;
        }
        
        private IEnumerator MonitorQuitProcess()
        {
            Debug.Log($"SessionLogger: Waiting up to {maxQuitWaitTime} seconds for logs to be sent...");
            
            while (!logSendComplete && Time.realtimeSinceStartup - quitStartTime < maxQuitWaitTime)
            {
                yield return null;
            }
            
            // If we reached the timeout and log is still not sent, force quit
            if (!logSendComplete)
            {
                Debug.LogWarning($"SessionLogger: Log sending timed out after {maxQuitWaitTime} seconds. Proceeding with application quit.");
            }
            
            // Actually quit the application since our handler will now return true
            Application.Quit();
        }
        
        private void OnLogSendComplete(bool success)
        {
            logSendComplete = true;
            Debug.Log($"SessionLogger: Log sending completed with status: {(success ? "Success" : "Failed")}");
            
            // In case we're not in the process of quitting, we don't need to do anything else
            if (!isQuitting) return;
            
            // Allow the application to quit by calling it again (now our handler will return true)
            Application.Quit();
        }

        private void InitializeSession()
        {
            appName = Application.productName;
            appVersion = Application.version;
            bundleVersionCode = appVersion;
            sessionStartTime = DateTime.UtcNow.ToString("o");
            sessionStartTimestamp = Time.realtimeSinceStartup;
            sessionActionCounts.Clear();
            logEntries.Clear();
            uniqueLogs.Clear();
            highestAvgFps = 0f;
            lowestAvgFps = float.MaxValue;
            highFpsData = null;
            lowFpsData = null;

            // Initialize action counts dictionary from config.EventsNames
            if (config != null && config.EventsNames != null)
            {
                foreach (string eventName in config.EventsNames)
                {
                    if (!string.IsNullOrEmpty(eventName) && !sessionActionCounts.ContainsKey(eventName))
                    {
                        sessionActionCounts[eventName] = 0;
                    }
                    else if (sessionActionCounts.ContainsKey(eventName))
                    {
                        Debug.LogWarning($"SessionLoggerSetup: Duplicate event name '{eventName}' detected in EventsNames. Only the first occurrence will be used.");
                    }
                }
            }
            else
            {
                Debug.LogError("SessionLogger config or EventsNames is null during initialization!");
            }
        }

        // Method now takes a string eventName
        public void LogEvent(string eventName)
        {
            if (!enabled) return;

            // Increment the count for this action name
            if (sessionActionCounts.ContainsKey(eventName))
            {
                sessionActionCounts[eventName]++;
            }
            else
            {
                // Log a warning if the event name is not pre-defined in the config
                Debug.LogWarning($"SessionLogger: Logged event '{eventName}' which was not defined in SessionLoggerSetup.EventsNames. It will be logged but not included in completion percentage unless added to setup.");
            }

            logEntries.Add(new LogEntry
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                type = eventName,
                message = "SessionEvent"
            });
        }
        
        // New method to log custom events with values
        public void LogCustomEvent(string eventName, string eventValue, bool overwrite)
        {
            if (!enabled) return;
            
            if (string.IsNullOrEmpty(eventName))
            {
                Debug.LogWarning("SessionLogger: LogCustomEvent called with empty eventName.");
                return;
            }
            
            if (overwrite || !customEvents.ContainsKey(eventName))
            {
                customEvents[eventName] = eventValue ?? string.Empty;
                Debug.Log($"SessionLogger: Custom event logged - {eventName}: {eventValue}");
            }
            else
            {
                Debug.Log($"SessionLogger: Custom event '{eventName}' already exists with value '{customEvents[eventName]}'. Use overwrite=true to update.");
            }
            
            logEntries.Add(new LogEntry
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                type = $"{eventName} [{eventValue}] overwrite:{overwrite}",
                message = "SessionCustomEvent"
            });
        }

        private void HandleLog(string condition, string stackTrace, LogType type)
        {
            if (type == LogType.Log) return;

            string uniqueKey = type + condition;
            if (uniqueLogs.Contains(uniqueKey)) return;

            uniqueLogs.Add(uniqueKey);
            logEntries.Add(new LogEntry
            {
                timestamp = DateTime.UtcNow.ToString("o"),
                type = type.ToString(),
                message = condition + (type == LogType.Exception || type == LogType.Error ? "\nStackTrace: " + stackTrace : "")
            });
        }
        
        private IEnumerator PeriodicSaveCoroutine()
        {
            if (!enabled || config.UpdateLogTime <= 0) yield break;
            
            Debug.Log($"SessionLogger: Starting periodic save every {config.UpdateLogTime} seconds.");
            
            while (true)
            {
                yield return new WaitForSecondsRealtime(config.UpdateLogTime);
                
                if (enabled && (currentModeConfig.saveLocalJson || currentModeConfig.sendToServer))
                {
                    Debug.Log("SessionLogger: Performing periodic save.");
                    SaveAndSend();
                }
            }
        }

        private IEnumerator FpsSamplingCoroutine()
        {
            if (!enabled) yield break;

            List<float> fpsBuffer = new List<float>();
            const int bufferSize = 10;
            const float sampleInterval = 1.0f;

            while (true)
            {
                float fps = 1f / Time.unscaledDeltaTime;
                fpsBuffer.Add(fps);

                if (fpsBuffer.Count >= bufferSize)
                {
                    float avg = fpsBuffer.Average();
                    Transform cam = Camera.main?.transform;
                    if (cam != null)
                    {
                        if (avg > highestAvgFps)
                        {
                            highestAvgFps = avg;
                            highFpsData = new FpsData(avg, cam.position, cam.rotation, SceneManager.GetActiveScene().name);
                        }
                        if (avg > 1.0f && avg < lowestAvgFps)
                        {
                            lowestAvgFps = avg;
                            lowFpsData = new FpsData(avg, cam.position, cam.rotation, SceneManager.GetActiveScene().name);
                        }
                    }
                    fpsBuffer.Clear();
                }
                yield return new WaitForSecondsRealtime(sampleInterval);
            }
        }

        private void OnApplicationQuit()
        {
            // We no longer call SaveAndSend directly here, as we handle quitting via the registered handler
            // This event is still useful for cleanup if needed
        }

        private void CheckForUnsentLogs()
        {
            if (!currentModeConfig.sendToServer) return;

            try
            {
                string[] potentialLogs = Directory.GetFiles(Application.persistentDataPath, "*_SessionLog.json");
                foreach (string logPath in potentialLogs)
                {
                    if (Path.GetFileNameWithoutExtension(logPath).StartsWith(fileTimestamp))
                    {
                        continue;
                    }
                    Debug.Log($"Found previous unsent log file: {logPath}. Attempting to send.");
                    try
                    {
                        string previousJson = File.ReadAllText(logPath);
                        StartCoroutine(SendToServer(previousJson, logPath, true, null));
                    }
                    catch (Exception readEx)
                    {
                        Debug.LogError($"Error reading previous log file '{logPath}': {readEx.Message}");
                    }
                }
            }
            catch (Exception dirEx)
            {
                Debug.LogError($"Error scanning for unsent logs: {dirEx.Message}");
            }
        }

        // New version with callback support
        public void SaveAndSendWithCallback(Action<bool> onComplete)
        {
            if (!enabled)
            {
                onComplete?.Invoke(false);
                return;
            }
            
            // Do the normal save
            string json = SaveLogToJson(out string filePath, out bool fileSaved);
            
            // If we need to send and file is saved (or we're sending without saving), do that with callback
            if (currentModeConfig.sendToServer && (fileSaved || !currentModeConfig.saveLocalJson))
            {
                if (string.IsNullOrEmpty(currentServerUrl))
                {
                    Debug.LogError("SessionLogger: Sending enabled, but Server URL is not configured in SessionLoggerSetup!");
                    onComplete?.Invoke(false);
                    return;
                }
                
                string pathToDelete = fileSaved ? filePath : null;
                StartCoroutine(SendToServer(json, pathToDelete, false, onComplete));
            }
            else
            {
                // No need to send, just invoke callback with the result of the save
                onComplete?.Invoke(fileSaved);
            }
        }

        // Original SaveAndSend method, modified to use the common SaveLogToJson method
        public void SaveAndSend()
        {
            if (!enabled) return;
            if (!currentModeConfig.saveLocalJson && !currentModeConfig.sendToServer)
            {
                Debug.Log("SessionLogger: Neither saving nor sending is enabled for this mode. Skipping.");
                return;
            }

            string json = SaveLogToJson(out string filePath, out bool fileSaved);
            
            if (currentModeConfig.sendToServer && (fileSaved || !currentModeConfig.saveLocalJson))
            {
                if (string.IsNullOrEmpty(currentServerUrl))
                {
                    Debug.LogError("SessionLogger: Sending enabled, but Server URL is not configured in SessionLoggerSetup!");
                    return;
                }
                string pathToDelete = fileSaved ? filePath : null;
                StartCoroutine(SendToServer(json, pathToDelete, false, null));
            }
        }
        
        // Common method to prepare and save the JSON
        private string SaveLogToJson(out string filePath, out bool fileSaved)
        {
            sessionDuration = Time.realtimeSinceStartup - sessionStartTimestamp;

            // Prepare ActionsReceived list and calculate completion percentage
            List<SerializableActionCount> actionsReceivedList = new List<SerializableActionCount>();
            int totalDefinedActionTypes = (config != null && config.EventsNames != null) ? config.EventsNames.Length : 0;
            int completedActionCount = 0;

            if (totalDefinedActionTypes > 0)
            {
                foreach (string actionName in config.EventsNames)
                {
                    if (string.IsNullOrEmpty(actionName)) continue;

                    int count = sessionActionCounts.ContainsKey(actionName) ? sessionActionCounts[actionName] : 0;
                    actionsReceivedList.Add(new SerializableActionCount { actionName = actionName, count = count });
                    if (count > 0)
                    {
                        completedActionCount++;
                    }
                }
            }

            float completedPercentage = (totalDefinedActionTypes > 0) ? (float)completedActionCount / totalDefinedActionTypes : 0f;
            
            // Convert custom events dictionary to list
            List<CustomEvent> customEventsList = new List<CustomEvent>();
            foreach (var kvp in customEvents)
            {
                customEventsList.Add(new CustomEvent { eventName = kvp.Key, eventValue = kvp.Value });
            }

            SessionData data = new()
            {
                appName = appName,
                version = appVersion,
                bundleVersionCode = bundleVersionCode,
                sessionStart = sessionStartTime,
                sessionDurationSeconds = sessionDuration,
                avgHighFps = highFpsData,
                avgLowFps = (lowestAvgFps == float.MaxValue) ? null : lowFpsData,
                logs = logEntries,
                ActionsReceived = actionsReceivedList,
                Completed_Percentage = completedPercentage,
                customEvents = customEventsList
            };

            string json = JsonUtility.ToJson(data, true);
            filePath = GetLogFilePath();
            fileSaved = false;

            if (currentModeConfig.saveLocalJson)
            {
                try
                {
                    File.WriteAllText(filePath, json);
                    Debug.Log($"Session log saved locally to: {filePath}");
                    fileSaved = true;
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error writing session log file: {e.Message}");
                    fileSaved = false;
                }
            }
            else
            {
                Debug.Log("SessionLogger: Local save disabled by configuration.");
            }
            
            return json;
        }

        // Modified to accept a callback parameter
        private IEnumerator SendToServer(string json, string filePathToDeleteOnSuccess, bool isRetry, Action<bool> onComplete)
        {
            if (!enabled || string.IsNullOrEmpty(currentServerUrl))
            {
                onComplete?.Invoke(false);
                yield break;
            }

            using (UnityWebRequest req = new UnityWebRequest(currentServerUrl, "POST"))
            {
                byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(json);
                req.uploadHandler = new UploadHandlerRaw(bodyRaw);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");

                if (!string.IsNullOrEmpty(config.serverApiKey) && !string.IsNullOrEmpty(config.serverApiKeyHeader))
                {
                    req.SetRequestHeader(config.serverApiKeyHeader, config.serverApiKey);
                }

                yield return req.SendWebRequest();

                bool success = false;
                
                if (req.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Session data {(isRetry ? "(retry)" : "")} sent successfully to {currentServerUrl}.");
                    if (!string.IsNullOrEmpty(filePathToDeleteOnSuccess))
                    {
                        try
                        {
                            if (File.Exists(filePathToDeleteOnSuccess))
                                File.Delete(filePathToDeleteOnSuccess);
                            Debug.Log($"Deleted successfully sent log file: {filePathToDeleteOnSuccess}");
                        }
                        catch (Exception e)
                        {
                            Debug.LogError($"Error deleting sent log file '{filePathToDeleteOnSuccess}': {e.Message}");
                        }
                    }
                    success = true;
                }
                else
                {
                    string logReference = string.IsNullOrEmpty(filePathToDeleteOnSuccess) ? "(not saved locally)" : $"Log saved locally at {filePathToDeleteOnSuccess}. Will retry next launch if applicable.";
                    Debug.LogWarning($"Failed to send session data {(isRetry ? "(retry)" : "")}. Error: {req.error}. Response Code: {req.responseCode}. {logReference}");
                    success = false;
                }
                
                // Invoke the callback if provided
                onComplete?.Invoke(success);
            }
        }

        // --- Nested Classes ---
        [Serializable]
        private class LogEntry { public string timestamp; public string type; public string message; }
        [Serializable]
        private class FpsData { public float avg; public Vector3 position; public Quaternion rotation; public string scene; public FpsData(float a, Vector3 p, Quaternion r, string s) { avg = a; position = p; rotation = r; scene = s; } }

        [Serializable]
        public struct CustomEvent
        {
            public string eventName;
            public string eventValue;
        }
        
        [Serializable]
        private class SessionData
        {
            public string appName;
            public string version;
            public string bundleVersionCode;
            public string sessionStart;
            public float sessionDurationSeconds;
            public FpsData avgHighFps;
            public FpsData avgLowFps;
            public List<LogEntry> logs;
            public List<SerializableActionCount> ActionsReceived;
            public float Completed_Percentage;
            public List<CustomEvent> customEvents;
        }
    }
} 
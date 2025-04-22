using UnityEngine;
using System;

namespace Inimart.SessionLogger
{
    [CreateAssetMenu(fileName = "SessionLoggerSetup", menuName = "Session Logger/Setup", order = 1)]
    public class SessionLoggerSetup : ScriptableObject
    {
        [Serializable]
        public struct ModeConfig
        {
            [Tooltip("Should the session log JSON file be saved locally?")]
            public bool saveLocalJson;
            
            [Tooltip("Should the session log be sent to the remote server? (Only works if Save Local JSON is also enabled)")]
            public bool sendToServer;
        }

        [Header("Editor Settings")]
        public ModeConfig editorConfig = new ModeConfig { saveLocalJson = true, sendToServer = false }; // Default: Save in editor, don't send

        [Header("Build Settings")]
        public ModeConfig buildConfig = new ModeConfig { saveLocalJson = true, sendToServer = true }; // Default: Save in build, send

        [Header("Server Configuration")]
        [Tooltip("The URL of the server endpoint to receive session logs.")]
        public string serverUrl = "https://receivesessionlog-146752482847.europe-west8.run.app";

        [Tooltip("(Optional) API Key or other header value for server authentication.")]
        public string serverApiKey = "";
        
        [Tooltip("(Optional) The name of the header field for the API key (e.g., 'X-API-Key').")]
        public string serverApiKeyHeader = "X-API-Key";

        [Header("Session Actions")]
        [Tooltip("Define the names of all possible actions to be logged in this session type.")]
        public string[] ActionNames = new string[] { // Example default actions
            "Scene_00_Loaded",
            "Scene_01_Loaded",
            "TutorialCompleted",
            "ItemCollected",
            "TaskComplete"
        };
    }
} 
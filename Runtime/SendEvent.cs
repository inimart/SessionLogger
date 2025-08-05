using UnityEngine;

namespace Inimart.SessionLogger
{
    /// <summary>
    /// Component to send predefined events from SessionLoggerSetup
    /// </summary>
    public class SendEvent : MonoBehaviour
    {
        [Header("Event Selection")]
        [Tooltip("Select an event from the dropdown to send")]
        public int selectedEventIndex = 0;
        
        [Header("Auto Send")]
        [Tooltip("Automatically send the event when this GameObject is enabled")]
        public bool fireOnEnable = false;
        
        // Private cached reference
        private SessionLoggerSetup config;
        private string[] eventNames = new string[0];
        
        void OnValidate()
        {
            LoadConfiguration();
        }
        
        void Awake()
        {
            LoadConfiguration();
        }
        
        void OnEnable()
        {
            if (fireOnEnable)
            {
                Send();
            }
        }
        
        private void LoadConfiguration()
        {
            // If no override, try to load from Resources
            if (config == null)
            {
                config = Resources.Load<SessionLoggerSetup>("SessionLoggerSetup");
            }
            
            // Update event names array
            if (config != null && config.EventsNames != null && config.EventsNames.Length > 0)
            {
                eventNames = config.EventsNames;
                
                // Validate selected index
                if (selectedEventIndex >= eventNames.Length)
                {
                    selectedEventIndex = 0;
                }
            }
            else
            {
                eventNames = new string[0];
                selectedEventIndex = 0;
            }
        }
        
        /// <summary>
        /// Send the selected event to SessionLogger
        /// </summary>
        public void Send()
        {
            if (SessionLogger.Instance == null)
            {
                UnityEngine.Debug.LogError("SendEvent: SessionLogger.Instance is null. Make sure SessionLogger is in the scene.");
                return;
            }
            
            if (eventNames.Length == 0)
            {
                UnityEngine.Debug.LogError("SendEvent: No events configured in SessionLoggerSetup.");
                return;
            }
            
            if (selectedEventIndex < 0 || selectedEventIndex >= eventNames.Length)
            {
                UnityEngine.Debug.LogError($"SendEvent: Invalid event index {selectedEventIndex}");
                return;
            }
            
            string eventName = eventNames[selectedEventIndex];
            SessionLogger.Instance.LogEvent(eventName);
            UnityEngine.Debug.Log($"SendEvent: Sent event '{eventName}'");
        }
        
        /// <summary>
        /// Get the currently selected event name (used by custom editor)
        /// </summary>
        public string GetSelectedEventName()
        {
            if (eventNames.Length == 0 || selectedEventIndex < 0 || selectedEventIndex >= eventNames.Length)
                return "(No event selected)";
            
            return eventNames[selectedEventIndex];
        }
        
        /// <summary>
        /// Get all available event names (used by custom editor)
        /// </summary>
        public string[] GetEventNames()
        {
            return eventNames ?? new string[0];
        }
    }
}
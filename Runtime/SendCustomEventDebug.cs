using UnityEngine;

namespace Inimart.SessionLogger
{
    /// <summary>
    /// Debug component to test LogCustomEvent functionality
    /// </summary>
    public class SendCustomEventDebug : MonoBehaviour
    {
        [Header("Custom Event Configuration")]
        [Tooltip("The name of the custom event")]
        public string eventName = "CustomEvent";
        
        [Tooltip("The value associated with this event")]
        public string eventValue = "Value";
        
        [Tooltip("If true, will overwrite existing events with the same name")]
        public bool overwrite = false;
        
        /// <summary>
        /// Send the custom event to SessionLogger
        /// </summary>
        public void SendCustomEvent()
        {
            if (SessionLogger.Instance == null)
            {
                UnityEngine.Debug.LogError("SendCustomEventDebug: SessionLogger.Instance is null. Make sure SessionLogger is in the scene.");
                return;
            }
            
            if (string.IsNullOrEmpty(eventName))
            {
                UnityEngine.Debug.LogError("SendCustomEventDebug: Event name cannot be empty.");
                return;
            }
            
            SessionLogger.Instance.LogCustomEvent(eventName, eventValue, overwrite);
            UnityEngine.Debug.Log($"SendCustomEventDebug: Sent custom event '{eventName}' with value '{eventValue}' (overwrite: {overwrite})");
        }
    }
}
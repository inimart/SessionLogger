using UnityEngine;

namespace Inimart.SessionLogger
{
    /// <summary>
    /// Component to send custom events to SessionLogger
    /// </summary>
    public class SendCustomEvent : MonoBehaviour
    {
        [Header("Custom Event Configuration")]
        [Tooltip("The name of the custom event")]
        public string eventName = "CustomEvent";
        
        [Tooltip("The value associated with this event")]
        public string eventValue = "Value";
        
        [Tooltip("If true, will overwrite existing events with the same name")]
        public bool overwrite = false;
        
        [Header("Auto Send")]
        [Tooltip("Automatically send the event when this GameObject is enabled")]
        public bool fireOnEnable = false;
        
        void OnEnable()
        {
            if (fireOnEnable)
            {
                Send();
            }
        }
        
        /// <summary>
        /// Send the custom event to SessionLogger
        /// </summary>
        public void Send()
        {
            if (SessionLogger.Instance == null)
            {
                UnityEngine.Debug.LogError("SendCustomEvent: SessionLogger.Instance is null. Make sure SessionLogger is in the scene.");
                return;
            }
            
            if (string.IsNullOrEmpty(eventName))
            {
                UnityEngine.Debug.LogError("SendCustomEvent: Event name cannot be empty.");
                return;
            }
            
            SessionLogger.Instance.LogCustomEvent(eventName, eventValue, overwrite);
            UnityEngine.Debug.Log($"SendCustomEvent: Sent custom event '{eventName}' with value '{eventValue}' (overwrite: {overwrite})");
        }
    }
}
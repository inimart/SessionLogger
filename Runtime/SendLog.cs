using UnityEngine;
using UnityEngine.Serialization;

namespace Inimart.SessionLogger
{
    /// <summary>
    /// Component to send custom log entries to SessionLogger
    /// </summary>
    public class SendLog : MonoBehaviour
    {
        [Header("Log Configuration")]
        [Tooltip("The type/category of the log entry")]
        public string Type = "Info";
        
        [Tooltip("The message content of the log entry")]
        public string Message = "Log message";
        
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
        /// Send a log entry using the public Type and Message fields
        /// </summary>
        public void Send()
        {
            if (SessionLogger.Instance == null)
            {
                UnityEngine.Debug.LogError("SendLog: SessionLogger.Instance is null. Make sure SessionLogger is in the scene.");
                return;
            }
            
            if (string.IsNullOrEmpty(Type))
            {
                UnityEngine.Debug.LogError("SendLog: Type cannot be empty.");
                return;
            }
            
            if (string.IsNullOrEmpty(Message))
            {
                UnityEngine.Debug.LogError("SendLog: Message cannot be empty.");
                return;
            }
            
            SessionLogger.Instance.AddLogEntry(Type, Message);
            UnityEngine.Debug.Log($"SendLog: Added log entry - Type: '{Type}', Message: '{Message}'");
        }
        
        /// <summary>
        /// Send a log entry with custom type and message parameters
        /// </summary>
        /// <param name="type">The type/category of the log entry</param>
        /// <param name="message">The message content of the log entry</param>
        public void Send(string type, string message)
        {
            if (SessionLogger.Instance == null)
            {
                UnityEngine.Debug.LogError("SendLog: SessionLogger.Instance is null. Make sure SessionLogger is in the scene.");
                return;
            }
            
            if (string.IsNullOrEmpty(type))
            {
                UnityEngine.Debug.LogError("SendLog: Type parameter cannot be empty.");
                return;
            }
            
            if (string.IsNullOrEmpty(message))
            {
                UnityEngine.Debug.LogError("SendLog: Message parameter cannot be empty.");
                return;
            }
            
            SessionLogger.Instance.AddLogEntry(type, message);
            UnityEngine.Debug.Log($"SendLog: Added log entry - Type: '{type}', Message: '{message}'");
        }
    }
}
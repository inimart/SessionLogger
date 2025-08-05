#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Inimart.SessionLogger.Debug;

namespace Inimart.SessionLogger.Editor
{
    [CustomEditor(typeof(SendEventDebug))]
    public class SendEventDebugEditor : UnityEditor.Editor
    {
        private SerializedProperty selectedEventIndexProp;
        private SerializedProperty configOverrideProp;
        
        void OnEnable()
        {
            selectedEventIndexProp = serializedObject.FindProperty("selectedEventIndex");
            configOverrideProp = serializedObject.FindProperty("configOverride");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            SendEventDebug sendEventDebug = (SendEventDebug)target;
            
            // Draw the config override field
            EditorGUILayout.PropertyField(configOverrideProp, new GUIContent("Config Override (Optional)", 
                "Optionally override the SessionLoggerSetup configuration"));
            
            EditorGUILayout.Space();
            
            // Get available event names
            string[] eventNames = sendEventDebug.GetEventNames();
            
            if (eventNames.Length == 0)
            {
                EditorGUILayout.HelpBox("No events configured. Please assign a SessionLoggerSetup with EventsNames configured, " +
                    "or place one in Resources/SessionLoggerSetup.", MessageType.Warning);
            }
            else
            {
                // Draw dropdown for event selection
                EditorGUILayout.LabelField("Event Selection", EditorStyles.boldLabel);
                
                int currentIndex = selectedEventIndexProp.intValue;
                if (currentIndex >= eventNames.Length)
                {
                    currentIndex = 0;
                    selectedEventIndexProp.intValue = 0;
                }
                
                EditorGUI.BeginChangeCheck();
                int newIndex = EditorGUILayout.Popup("Select Event", currentIndex, eventNames);
                if (EditorGUI.EndChangeCheck())
                {
                    selectedEventIndexProp.intValue = newIndex;
                }
                
                // Show currently selected event
                EditorGUILayout.HelpBox($"Selected: {sendEventDebug.GetSelectedEventName()}", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Draw the send button
            GUI.enabled = eventNames.Length > 0 && Application.isPlaying;
            if (GUILayout.Button("Send Event", GUILayout.Height(30)))
            {
                sendEventDebug.SendEvent();
            }
            GUI.enabled = true;
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to send events.", MessageType.Info);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
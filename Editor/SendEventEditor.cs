#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Inimart.SessionLogger;

namespace Inimart.SessionLogger.Editor
{
    [CustomEditor(typeof(SendEvent))]
    public class SendEventEditor : UnityEditor.Editor
    {
        private SerializedProperty selectedEventIndexProp;
        private SerializedProperty fireOnEnableProp;
        
        void OnEnable()
        {
            selectedEventIndexProp = serializedObject.FindProperty("selectedEventIndex");
            fireOnEnableProp = serializedObject.FindProperty("fireOnEnable");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            SendEvent sendEvent = (SendEvent)target;
            
            // Get available event names
            string[] eventNames = sendEvent.GetEventNames();
            
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
                EditorGUILayout.HelpBox($"Selected: {sendEvent.GetSelectedEventName()}", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Auto send option
            EditorGUILayout.LabelField("Auto Send", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fireOnEnableProp, new GUIContent("Fire On Enable", 
                "Automatically send the event when this GameObject is enabled"));
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
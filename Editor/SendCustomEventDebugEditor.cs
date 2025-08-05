#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Inimart.SessionLogger.Debug;

namespace Inimart.SessionLogger.Editor
{
    [CustomEditor(typeof(SendCustomEventDebug))]
    public class SendCustomEventDebugEditor : UnityEditor.Editor
    {
        private SerializedProperty eventNameProp;
        private SerializedProperty eventValueProp;
        private SerializedProperty overwriteProp;
        
        void OnEnable()
        {
            eventNameProp = serializedObject.FindProperty("eventName");
            eventValueProp = serializedObject.FindProperty("eventValue");
            overwriteProp = serializedObject.FindProperty("overwrite");
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            SendCustomEventDebug sendCustomEventDebug = (SendCustomEventDebug)target;
            
            EditorGUILayout.LabelField("Custom Event Configuration", EditorStyles.boldLabel);
            
            // Event Name field
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(eventNameProp, new GUIContent("Event Name", 
                "The name of the custom event to log"));
            if (EditorGUI.EndChangeCheck() && string.IsNullOrEmpty(eventNameProp.stringValue))
            {
                EditorGUILayout.HelpBox("Event name cannot be empty!", MessageType.Error);
            }
            
            // Event Value field
            EditorGUILayout.PropertyField(eventValueProp, new GUIContent("Event Value", 
                "The value associated with this event"));
            
            // Overwrite toggle
            EditorGUILayout.PropertyField(overwriteProp, new GUIContent("Overwrite", 
                "If enabled, will overwrite existing events with the same name"));
            
            EditorGUILayout.Space();
            
            // Preview of what will be sent
            if (!string.IsNullOrEmpty(sendCustomEventDebug.eventName))
            {
                string preview = $"Will log: {sendCustomEventDebug.eventName} = {sendCustomEventDebug.eventValue}";
                if (sendCustomEventDebug.overwrite)
                {
                    preview += " (overwriting if exists)";
                }
                EditorGUILayout.HelpBox(preview, MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            // Draw the send button
            GUI.enabled = !string.IsNullOrEmpty(sendCustomEventDebug.eventName) && Application.isPlaying;
            if (GUILayout.Button("Send Custom Event", GUILayout.Height(30)))
            {
                sendCustomEventDebug.SendCustomEvent();
            }
            GUI.enabled = true;
            
            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Enter Play Mode to send custom events.", MessageType.Info);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif
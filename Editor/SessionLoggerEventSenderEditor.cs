#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Inimart.SessionLogger;

namespace Inimart.SessionLogger.Editor
{
    [CustomEditor(typeof(SessionLoggerEventSender))]
    public class SessionLoggerEventSenderEditor : UnityEditor.Editor
    {
        private SerializedProperty actionToLogProp;
        private SerializedProperty configOverrideProp;
        private SerializedProperty fireOnEnableProp;
        private SessionLoggerSetup config;
        private string[] actionNames = new string[0]; // Default empty array
        private int selectedIndex = -1;

        private void OnEnable()
        {
            actionToLogProp = serializedObject.FindProperty("actionToLog");
            configOverrideProp = serializedObject.FindProperty("optionalConfigOverride");
            fireOnEnableProp = serializedObject.FindProperty("FireOnEnable");
            LoadConfigAndActions();
            UpdateSelectedIndex();
        }

        private void LoadConfigAndActions()
        {
            // Try loading from override first
            config = configOverrideProp.objectReferenceValue as SessionLoggerSetup;

            // If override is null, try loading from Resources
            if (config == null)
            {
                config = Resources.Load<SessionLoggerSetup>("SessionLoggerSetup");
            }

            // Populate action names if config is loaded
            if (config != null && config.ActionNames != null)
            {
                actionNames = config.ActionNames;
            }
            else
            {
                actionNames = new string[0]; // Reset if no config
            }
        }

        private void UpdateSelectedIndex()
        {
            selectedIndex = -1; // Default to none selected
            if (actionNames.Length > 0 && !string.IsNullOrEmpty(actionToLogProp.stringValue))
            {
                for (int i = 0; i < actionNames.Length; i++)
                {
                    if (actionNames[i] == actionToLogProp.stringValue)
                    {
                        selectedIndex = i;
                        break;
                    }
                }
                // If the saved string is not in the current list, reset selection
                if (selectedIndex == -1)
                {
                    // actionToLogProp.stringValue = ""; // Optionally clear the invalid string
                }
            }
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw the optional config override field
            EditorGUILayout.PropertyField(configOverrideProp);

            // Draw the FireOnEnable toggle
            EditorGUILayout.PropertyField(fireOnEnableProp, new GUIContent("Fire On Enable", 
                "When enabled, the configured event will be sent automatically when this component is enabled."));

            // Detect changes in the override field to reload actions
            EditorGUI.BeginChangeCheck();
            SessionLoggerSetup newConfig = configOverrideProp.objectReferenceValue as SessionLoggerSetup;
            if (EditorGUI.EndChangeCheck() || config != newConfig)
            {
                LoadConfigAndActions();
                UpdateSelectedIndex(); // Update index based on potentially new action names
            }

            // Display error if no config is found
            if (config == null)
            {
                EditorGUILayout.HelpBox("SessionLoggerSetup asset not found (assign override or place in Resources/SessionLoggerSetup).", MessageType.Error);
                actionNames = new string[0];
                selectedIndex = -1;
            }

            // Draw the action selection popup if config is loaded
            if (config != null)
            {
                if (actionNames.Length > 0)
                {
                    EditorGUI.BeginChangeCheck();
                    selectedIndex = EditorGUILayout.Popup("Action To Log", selectedIndex, actionNames);
                    if (EditorGUI.EndChangeCheck())
                    {
                        if (selectedIndex >= 0 && selectedIndex < actionNames.Length)
                        {
                            actionToLogProp.stringValue = actionNames[selectedIndex];
                        }
                        else
                        {
                            actionToLogProp.stringValue = ""; // Clear if index is invalid
                        }
                    }
                    // If selection is valid, ensure string value matches (handles initial load)
                    else if (selectedIndex >= 0 && selectedIndex < actionNames.Length && actionToLogProp.stringValue != actionNames[selectedIndex])
                    {
                        actionToLogProp.stringValue = actionNames[selectedIndex];
                    }
                    // If selection is invalid, ensure string value is cleared
                    else if (selectedIndex < 0 && !string.IsNullOrEmpty(actionToLogProp.stringValue))
                    {
                        actionToLogProp.stringValue = "";
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("No actions defined in SessionLoggerSetup.ActionNames array.", MessageType.Warning);
                    selectedIndex = -1;
                    actionToLogProp.stringValue = "";
                }
            }
            else
            {
                // Display a disabled field when no config is available
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.TextField("Action To Log", "(Config not found)");
                EditorGUI.EndDisabledGroup();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
#endif 
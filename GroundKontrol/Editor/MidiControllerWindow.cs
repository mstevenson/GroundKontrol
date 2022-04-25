using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace GroundKontrol
{
    public class MidiControllerWindow : EditorWindow
    {
        [MenuItem("Window/MIDI Controller Mapping")]
        public static void ShowWindow()
        {
            GetWindow<MidiControllerWindow>("GroundKontrol", typeof(MidiControllerWindow));
        }
        
        private const int Width = 90;
        
        private Texture2D _whiteImage;
        private Texture2D _blackImage;

        private void Awake()
        {
            _whiteImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.lazerwalker.groundkontrol/korg-white.png");
            _blackImage = AssetDatabase.LoadAssetAtPath<Texture2D>("Packages/com.lazerwalker.groundkontrol/korg-black.png");
        }
        
        private void OnGUI()
        {
            DrawToolbar();
            
            // Horizontal row of knobs 1-8
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(350);
                
                foreach (var knob in GroundKontrol.instance.knobs)
                {
                    DrawControl(knob);
                }
            }
            
            // Image of MIDI controller
            GUILayout.Label(GroundKontrol.instance.showBlack ? _blackImage : _whiteImage);

            using (new EditorGUILayout.HorizontalScope())
            {
                DrawImageSwapButton();
                
                GUILayout.Space(200);

                // Horizontal row of sliders 1-8
                foreach (var slider in GroundKontrol.instance.sliders)
                {
                    DrawControl(slider);
                }
            }
        }

        private static void DrawToolbar()
        {
            using (new GUILayout.HorizontalScope(EditorStyles.toolbar))
            {
                if (GUILayout.Button("Clear", EditorStyles.toolbarButton, GUILayout.Width(50)))
                {
                    GroundKontrol.ClearAllBindings();
                }
                
                if (GUILayout.Button("Select Asset", EditorStyles.toolbarButton, GUILayout.Width(90)))
                {
                    Selection.activeObject = GroundKontrol.instance;
                }
                
                GUILayout.FlexibleSpace();
            }
        }
        
        private static void DrawImageSwapButton()
        {
            GUILayout.Space(10);
            var colorText = GroundKontrol.instance.showBlack ? "My controller is white!" : "My controller is black!";
            if (GUILayout.Button(colorText, GUILayout.Width(140)))
            {
                GroundKontrol.instance.showBlack = !GroundKontrol.instance.showBlack;
            }
        }
        
        private static void DrawControl(MidiInput midiInput)
        {
            using (new EditorGUILayout.VerticalScope(GUILayout.Width(Width)))
            {
                EditorGUILayout.LabelField(midiInput.Name, GUILayout.Width(Width));

                if (GUILayout.Button("+ Add New", GUILayout.Width(Width)))
                {
                    GroundKontrol.CreateBinding(midiInput);
                }

                EditorGUILayout.Space();
                
                var bindingsToRemove = new List<PropertyBinding>();
                foreach (var property in midiInput.propertyBindings)
                {
                    var exists = DrawPropertyBinding(property);
                    if (!exists)
                    {
                        bindingsToRemove.Add(property);
                    }
                }
                foreach (var binding in bindingsToRemove)
                {
                    GroundKontrol.RemoveBinding(midiInput, binding);
                }
            }
        }

        private static bool DrawPropertyBinding(PropertyBinding property)
        {
            EditorGUIUtility.labelWidth = 60.0f;

            if (GUILayout.Button("X", GUILayout.Width(20)))
            {
                return false;
            }

            var selectedGameObj = (GameObject) EditorGUILayout.ObjectField(property.GameObject, typeof(GameObject), true, GUILayout.Width(Width));
            if (selectedGameObj != null && selectedGameObj != property.GameObject)
            {
                property.GameObject = selectedGameObj;
            }
            
            using (new EditorGUILayout.VerticalScope())
            {
                using (new EditorGUI.DisabledScope(selectedGameObj == null))
                {
                    property.ComponentIndex = EditorGUILayout.Popup("", property.ComponentIndex, property.componentNames, EditorStyles.popup, GUILayout.Width(Width));
                }
                using (new EditorGUI.DisabledScope(property.ComponentIndex < 0))
                {
                    property.SerializedPropertyIndex = EditorGUILayout.Popup("", property.SerializedPropertyIndex, property.serializedPropertyPaths, EditorStyles.popup, GUILayout.Width(Width));
                }
                property.range = EditorGUILayout.DelayedIntField("Range", property.range, GUILayout.Width(Width));
                EditorGUILayout.LabelField("Value: ", property.midiValue.ToString(), GUILayout.Width(Width));
            }
            
            EditorGUILayout.Space();
            
            return true;
        }
    }
}
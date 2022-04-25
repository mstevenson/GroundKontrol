using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace GroundKontrol
{
    [Serializable]
    public class PropertyBinding
    {
        public float midiValue;
        public int range = 1;
        
        [SerializeField] private GameObject gameObject;
        
        // TODO this component reference will always be null during play if the component type is not accessible
        // from the GroundKontrol assembly, which is any component that is not built-in to Unity.
        [SerializeField] private Component component;
        [SerializeField] private List<Component> components = new List<Component>();
        public string[] componentNames = Array.Empty<string>();
        [SerializeField] private int componentIndex = -1;
        
        [SerializeField] private int serializedPropertyIndex = -1;
        [SerializeField] private string serializedPropertyPath;
        public string[] serializedPropertyPaths = Array.Empty<string>();

        public GameObject GameObject
        {
            get => gameObject;
            set
            {
                if (gameObject == value)
                {
                    return;
                }
            
                gameObject = value;
                ClearSerializedProperties();
                ClearComponents();
                CacheComponents(value);
            }
        }

        private void ClearComponents()
        {
            components.Clear();
            componentNames = Array.Empty<string>();
        }
        
        private void CacheComponents(GameObject obj)
        {
            components.AddRange(obj.GetComponents<Component>());
            if (components.Count > 0)
            {
                componentNames = components.Select(c => c.GetType().Name).ToArray();
            }
        }

        public Component Component
        {
            get => component;
            set
            {
                if (component == value)
                {
                    return;
                }
                
                _serializedObject = null;
                
                component = value;
                ClearSerializedProperties();
                if (component != null)
                {
                    CacheSerializedProperties();
                }
            }
        }
        
        private SerializedObject _serializedObject;
        public SerializedObject SerializedObject
        {
            get
            {
                if (component == null)
                {
                    return null;
                }
                if (_serializedObject == null)
                {
                    _serializedObject = new SerializedObject(component);
                }
                return _serializedObject;
            }
        }

        public int ComponentIndex
        {
            get => componentIndex;
            set
            {
                if (value == componentIndex || gameObject == null)
                {
                    return;
                }
    
                componentIndex = value;
                Component = gameObject.GetComponents<Component>()[value];
            }
        }
        
        public SerializedProperty SerializedProperty
        {
            get
            {
                if (SerializedObject == null)
                {
                    return null;
                }
                return SerializedObject.FindProperty(serializedPropertyPath);
            }
            private set => serializedPropertyPath = value.propertyPath;
        }
        
        private void ClearSerializedProperties()
        {
            serializedPropertyIndex = -1;
            serializedPropertyPath = null;
            serializedPropertyPaths = Array.Empty<string>();
        }

        private void CacheSerializedProperties()
        {
            var propertyPathsList = new List<string>();
            var enterChildren = true;
            var property = SerializedObject.GetIterator();
            while (property.NextVisible(enterChildren))
            {
                enterChildren = false;
                // TODO filter out properties with m_ prefix, and support setters for Unity's build in components
                if (property.propertyType != SerializedPropertyType.Integer &&
                    property.propertyType != SerializedPropertyType.Boolean &&
                    property.propertyType != SerializedPropertyType.Float &&
                    property.propertyType != SerializedPropertyType.Enum)
                {
                    continue;
                }
                propertyPathsList.Add(property.propertyPath);
            }
            serializedPropertyPaths = propertyPathsList.ToArray();
        }

        public int SerializedPropertyIndex
        {
            get => serializedPropertyIndex;
            set
            {
                if (serializedPropertyIndex == value)
                {
                    return;
                }
                serializedPropertyIndex = value;
                serializedPropertyPath = serializedPropertyPaths[value];
            }
        }

        public void ApplyMidiValueToSerializedProperty()
        {
            if (serializedPropertyIndex == -1)
            {
                return;
            }
            
            switch (SerializedProperty.propertyType)
            {
                case SerializedPropertyType.Float:
                    SerializedProperty.floatValue = midiValue;
                    break;
                case SerializedPropertyType.Integer:
                    SerializedProperty.intValue = Mathf.RoundToInt(midiValue);
                    break;
                case SerializedPropertyType.Boolean:
                    SerializedProperty.boolValue = midiValue > range / 2f;
                    break;
                case SerializedPropertyType.Enum:
                    SerializedProperty.enumValueIndex = Mathf.RoundToInt(midiValue);
                    break;
            }

            SerializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(SerializedObject.targetObject);
        }

        public void RestoreMidiValueFromSerializedProperty()
        {
            if (serializedPropertyIndex == -1)
            {
                return;
            }

            midiValue = SerializedProperty.propertyType switch
            {
                SerializedPropertyType.Float => SerializedProperty.floatValue,
                SerializedPropertyType.Integer => SerializedProperty.intValue,
                SerializedPropertyType.Boolean => SerializedProperty.boolValue ? range : 0,
                SerializedPropertyType.Enum => SerializedProperty.enumValueIndex,
                _ => midiValue
            };
        }
    }
}

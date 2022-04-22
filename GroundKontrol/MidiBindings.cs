using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class Binding
{
    public Component component;
    public string propertyPath;
    public SerializedObject serializedObject;
    public SerializedProperty serializedProperty;
}

[Serializable]
public class FloatBinding : Binding
{
    public float value;
}

[FilePath("MidiBindings.asset", FilePathAttribute.Location.ProjectFolder)]
public class MidiBindings : ScriptableSingleton<MidiBindings>
{
    [SerializeField] private List<FloatBinding> floatBindings = new List<FloatBinding>();
    
    private void OnEnable()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }
    
    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange mode)
    {
        if (mode == PlayModeStateChange.ExitingPlayMode)
        {
            foreach (var binding in instance.floatBindings)
            {
                binding.value = binding.serializedProperty.floatValue;
            }
        }
        else if (mode == PlayModeStateChange.EnteredEditMode)
        {
            foreach (var binding in instance.floatBindings)
            {
                binding.serializedProperty.floatValue = binding.value;
                binding.serializedObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(binding.serializedObject.targetObject);
            }
        }
    }
    
    public void AddBinding(Component component, string propertyPath)
    {
        var serializedObject = new SerializedObject(component);
        var serializedProperty = serializedObject.FindProperty(propertyPath);
        Assert.AreEqual("float", serializedProperty.type);
        var binding = new FloatBinding
        {
            component = component,
            propertyPath = propertyPath
        };
        floatBindings.Add(binding);
    }
    
    public void RemoveBinding(Component component, string propertyPath)
    {
        var binding = floatBindings.Find(b => b.component == component && b.propertyPath == propertyPath);
        if (binding != null)
        {
            floatBindings.Remove(binding);
        }
    }

    private Binding GetOrCreateBinding(Component target, string propertyPath)
    {
        foreach (var binding in floatBindings)
        {
            if (binding.component == target && binding.propertyPath == propertyPath)
            {
                if (binding.serializedObject == null)
                {
                    binding.serializedObject = new SerializedObject(target);
                    binding.serializedProperty = binding.serializedObject.FindProperty(propertyPath);
                }
                return binding;
            }
        }

        return null;
    }
    
    public void SetFloat(Component target, string serializedPropertyPath, float value)
    {
        var binding = GetOrCreateBinding(target, serializedPropertyPath);
        binding.serializedProperty.floatValue = value;
        binding.serializedObject.ApplyModifiedPropertiesWithoutUndo();
    }
    
    
    // Debugging
    
    [MenuItem("Midi/Add binding")]
    public static void AddBinding()
    {
        instance.AddBinding(GameObject.FindObjectOfType<Tester>(), "time");
    }
    
    [MenuItem("Midi/Show bindings")]
    public static void ShowBindings()
    {
        Selection.activeObject = instance;
    }
    
    [MenuItem("Midi/Clear bindings")]
    public static void ClearBindings()
    {
        instance.floatBindings.Clear();
        EditorUtility.SetDirty(instance);
    }
}

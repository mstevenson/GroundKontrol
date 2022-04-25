using System;
using System.Collections.Generic;
using System.Linq;
using MidiJack;
using UnityEditor;
using UnityEngine;

namespace GroundKontrol
{
    [FilePath("GroundKontrol.asset", FilePathAttribute.Location.ProjectFolder)]
    [Serializable]
    public class GroundKontrol : ScriptableSingleton<GroundKontrol>
    {
        public bool showBlack;
        
        public MidiInput[] knobs = Enumerable.Range(0, 8).Select(i => new MidiInput(MidiInputType.Knob, i)).ToArray();
        public MidiInput[] sliders = Enumerable.Range(0, 8).Select(i => new MidiInput(MidiInputType.Slider, i)).ToArray();

        private IEnumerable<MidiInput> AllMidiInputs => knobs.Concat(sliders);
        
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
            // Start playing
            if (mode == PlayModeStateChange.EnteredPlayMode)
            {
                EditorApplication.update += OnUpdate;
                OnUpdate();
                // foreach (var midiInput in instance.AllMidiInputs)
                // {
                //     foreach (var binding in midiInput.propertyBindings)
                //     {
                //         binding.RestoreMidiValueFromSerializedProperty();
                //     }
                // }
            }
            // Stop playing
            else if (mode == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.update -= OnUpdate;
                foreach (var midiInput in instance.AllMidiInputs)
                {
                    foreach (var binding in midiInput.propertyBindings)
                    {
                        binding.ApplyMidiValueToSerializedProperty();
                    }
                }
            }
        }

        private static void OnUpdate()
        {
            if (!Application.isPlaying)
            {
                return;
            }
            
            // var shouldChange = !(MidiMaster.GetKnob(MidiChannel.All, 43) > 0.0);

            EditorApplication.QueuePlayerLoopUpdate();

            foreach (var knob in instance.knobs)
            {
                UpdateInput(knob);
            }
            foreach (var slider in instance.sliders)
            {
                UpdateInput(slider);
            }
        }

        private static void UpdateInput(MidiInput midiInput)
        {
            foreach (var propertyBinding in midiInput.propertyBindings)
            {
                instance.UpdateSerializedPropertyValue(midiInput, propertyBinding);
            }
        }
        
        private readonly Dictionary<float, float> _previousValues = new Dictionary<float, float>();
        
        private void UpdateSerializedPropertyValue(MidiInput midiInput, PropertyBinding property)
        {
            var previousValue = 0.0f;
            if (_previousValues.ContainsKey(midiInput.KnobNumber))
            {
                previousValue = _previousValues[midiInput.KnobNumber];
            }

            var knobValue = MidiMaster.GetKnob(MidiChannel.All, midiInput.KnobNumber) * property.range;
            var difference = knobValue - previousValue;
            var newValue = property.midiValue + difference;

            _previousValues[midiInput.KnobNumber] = knobValue;

            property.midiValue = newValue;
            property.ApplyMidiValueToSerializedProperty();
        }

        public static void CreateBinding(MidiInput midiInput)
        {
            var binding = new PropertyBinding();
            if (midiInput.propertyBindings.Contains(binding))
            {
                return;
            }
            midiInput.propertyBindings.Add(binding);
            EditorUtility.SetDirty(instance);
        }
        
        public static void RemoveBinding(MidiInput midiInput, PropertyBinding binding)
        {
            midiInput.propertyBindings.Remove(binding);
            EditorUtility.SetDirty(instance);
        }
        
        public static void ClearAllBindings()
        {
            foreach (var midiInput in instance.AllMidiInputs)
            {
                midiInput.propertyBindings.Clear();
            }
            EditorUtility.SetDirty(instance);
        }
    }
}


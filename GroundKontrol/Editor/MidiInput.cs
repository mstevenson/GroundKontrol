using System;
using System.Collections.Generic;

namespace GroundKontrol
{
    public enum MidiInputType
    {
        Slider,
        Knob
    }

    [Serializable]
    public class MidiInput
    {
        public MidiInputType type;
	
        public int number;
        
        public List<PropertyBinding> propertyBindings = new List<PropertyBinding>();

        public MidiInput(MidiInputType type, int number)
        {
            this.type = type;
            this.number = number;
        }

        public int KnobNumber =>
            type switch
            {
                MidiInputType.Knob => number + 16,
                MidiInputType.Slider => number,
                _ => throw new ArgumentOutOfRangeException()
            };

        public string Name => $"{type} {number + 1}";
    }
}

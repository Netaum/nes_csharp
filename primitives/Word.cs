using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nes_csharp.primitives
{
    public class Word
    {
        private const int VALUE_MASK = 7; // Mask to keep only the last 3 bits
        private const int STATE_MASK = 24; // Mask to keep the next 2 bits
        private const int SWITCH_1_MASK = 32; // Mask for the first switch bit
        private const int SWITCH_2_MASK = 64; // Mask for the second switch bit
        private const int UNUSED_MASK = 128; // Mask for the second switch bit

        private int internalValue = 0;

        public int Value
        {
            get { return internalValue & VALUE_MASK; }
            set { internalValue |= value & VALUE_MASK; }
        }

        public int State
        {
            get { return (internalValue & STATE_MASK) >> 3; }
            set { internalValue |= (value << 3) & STATE_MASK; }
        }

        public int Switch1
        {
            get { return (internalValue & SWITCH_1_MASK) >> 5; }
            set { internalValue |= (value << 5) & SWITCH_1_MASK; }
        }

        public int Switch2
        {
            get { return (internalValue & SWITCH_2_MASK) >> 6; }
            set { internalValue |= (value << 6) & SWITCH_2_MASK; }
        }

        public int Unused
        {
            get { return (internalValue & UNUSED_MASK) >> 7; }
            set { internalValue |= (value << 7) & UNUSED_MASK; }
        }
        
        public int InternalValue { get { return internalValue; } }
    }
}
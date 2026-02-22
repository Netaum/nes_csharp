using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emulator.Primitives
{
    public class ControlRegister
    {
        private const int NAMETABLE_X_MASK = 1 << 0;
        private const int NAMETABLE_Y_MASK = 1 << 1;
        private const int INCREMENT_MODE_MASK = 1 << 2;
        private const int SPRITE_PATTERN_TABLE_ADDRESS_MASK = 1 << 3;
        private const int BACKGROUND_PATTERN_TABLE_ADDRESS_MASK = 1 << 4;
        private const int SPRITE_SIZE_MASK = 1 << 5;
        private const int MASTER_SLAVE_SELECT_MASK = 1 << 6;
        private const int ENABLE_NMI_MASK = 1 << 7;

        private int internalValue = 0;

        public int NametableX
        {
            get { return internalValue & NAMETABLE_X_MASK; }
            set
            {
                internalValue = (internalValue & ~NAMETABLE_X_MASK) | (value & NAMETABLE_X_MASK);
            }
        }

        public int NametableY
        {
            get { return internalValue & NAMETABLE_Y_MASK; }
            set
            {
                internalValue = (internalValue & ~NAMETABLE_Y_MASK) | (value & NAMETABLE_Y_MASK);
            }   
        }

        public bool IncrementMode
        {
            get { return (internalValue & INCREMENT_MODE_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= INCREMENT_MODE_MASK;
                else
                    internalValue &= ~INCREMENT_MODE_MASK;
            }
        }

        public bool SpritePatternTableAddress
        {
            get { return (internalValue & SPRITE_PATTERN_TABLE_ADDRESS_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= SPRITE_PATTERN_TABLE_ADDRESS_MASK;
                else
                    internalValue &= ~SPRITE_PATTERN_TABLE_ADDRESS_MASK;
            }
        }

        public int BackgroundPatternTableAddress
        {
            get { return internalValue & BACKGROUND_PATTERN_TABLE_ADDRESS_MASK; }
            set
            {
                internalValue = (internalValue & ~BACKGROUND_PATTERN_TABLE_ADDRESS_MASK) | (value & BACKGROUND_PATTERN_TABLE_ADDRESS_MASK);
            }
        }

        public bool SpriteSize
        {
            get { return (internalValue & SPRITE_SIZE_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= SPRITE_SIZE_MASK;
                else
                    internalValue &= ~SPRITE_SIZE_MASK;
            }
        }

        public bool MasterSlaveSelect
        {
            get { return (internalValue & MASTER_SLAVE_SELECT_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= MASTER_SLAVE_SELECT_MASK;
                else
                    internalValue &= ~MASTER_SLAVE_SELECT_MASK;
            }
        }

        public bool EnableNMI
        {
            get { return (internalValue & ENABLE_NMI_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= ENABLE_NMI_MASK;
                else
                    internalValue &= ~ENABLE_NMI_MASK;
            }
        }

        public byte Value
        {
            get { return (byte)internalValue; }
            set { internalValue = value; }
        }

    }
}
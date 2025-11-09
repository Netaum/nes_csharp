namespace Emulator.Primitives
{
    public class StatusRegister
    {
        private int internalValue = 0;
        private const int UNUSED_MASK = 1 << 4;
        private const int OVERFLOW_MASK = 1 << 5;
        private const int SPRITE_ZERO_HIT_MASK = 1 << 6;
        private const int VERTICAL_BLANK_MASK = 1 << 7;

        public bool Overflow
        {
            get { return (internalValue & OVERFLOW_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= OVERFLOW_MASK;
                else
                    internalValue &= ~OVERFLOW_MASK;
            }
        }

        public bool SpriteZeroHit
        {
            get { return (internalValue & SPRITE_ZERO_HIT_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= SPRITE_ZERO_HIT_MASK;
                else
                    internalValue &= ~SPRITE_ZERO_HIT_MASK;
            }
        }

        public bool VerticalBlank
        {
            get { return (internalValue & VERTICAL_BLANK_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= VERTICAL_BLANK_MASK;
                else
                    internalValue &= ~VERTICAL_BLANK_MASK;
            }
        }

        public byte Value
        {
            get { return (byte)internalValue; }
            set { internalValue = value; }
        }
    }
}
namespace Emulator.Primitives
{
    public class MaskRegister
    {
        private const int GRAYSCALE_MASK = 1 << 0;
        private const int SHOW_BACKGROUND_LEFT_MASK = 1 << 1;
        private const int SHOW_SPRITES_LEFT_MASK = 1 << 2;
        private const int SHOW_BACKGROUND_MASK = 1 << 3;
        private const int SHOW_SPRITES_MASK = 1 << 4;
        private const int ENHANCE_RED_MASK = 1 << 5;
        private const int ENHANCE_GREEN_MASK = 1 << 6;
        private const int ENHANCE_BLUE_MASK = 1 << 7;
        private int internalValue = 0;

        public bool Grayscale
        {
            get { return (internalValue & GRAYSCALE_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= GRAYSCALE_MASK;
                else
                    internalValue &= ~GRAYSCALE_MASK;
            }
        }

        public bool ShowBackgroundLeft
        {
            get { return (internalValue & SHOW_BACKGROUND_LEFT_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= SHOW_BACKGROUND_LEFT_MASK;
                else
                    internalValue &= ~SHOW_BACKGROUND_LEFT_MASK;
            }
        }

        public bool ShowSpritesLeft
        {
            get { return (internalValue & SHOW_SPRITES_LEFT_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= SHOW_SPRITES_LEFT_MASK;
                else
                    internalValue &= ~SHOW_SPRITES_LEFT_MASK;
            }
        }

        public bool ShowBackground
        {
            get { return (internalValue & SHOW_BACKGROUND_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= SHOW_BACKGROUND_MASK;
                else
                    internalValue &= ~SHOW_BACKGROUND_MASK;
            }
        }

        public bool ShowSprites
        {
            get { return (internalValue & SHOW_SPRITES_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= SHOW_SPRITES_MASK;
                else
                    internalValue &= ~SHOW_SPRITES_MASK;
            }
        }

        public bool EnhanceRed
        {
            get { return (internalValue & ENHANCE_RED_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= ENHANCE_RED_MASK;
                else
                    internalValue &= ~ENHANCE_RED_MASK;
            }
        }

        public bool EnhanceGreen
        {
            get { return (internalValue & ENHANCE_GREEN_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= ENHANCE_GREEN_MASK;
                else
                    internalValue &= ~ENHANCE_GREEN_MASK;
            }
        }

        public bool EnhanceBlue
        {
            get { return (internalValue & ENHANCE_BLUE_MASK) != 0; }
            set
            {
                if (value)
                    internalValue |= ENHANCE_BLUE_MASK;
                else
                    internalValue &= ~ENHANCE_BLUE_MASK;
            }
        }
        public byte Value
        {
            get { return (byte)internalValue; }
            set { internalValue = value; }
        }
    }
}
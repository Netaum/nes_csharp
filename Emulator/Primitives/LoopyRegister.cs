using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emulator.Primitives
{
    public class LoopyRegister
    {
        private const int COARSE_X_MASK = 0b0000000000011111;
        private const int COARSE_Y_MASK = 0b0000001111100000;
        private const int NAMETABLE_X_MASK = 0b0000010000000000;
        private const int NAMETABLE_Y_MASK = 0b0000100000000000;
        private const int FINE_Y_MASK = 0b0111000000000000;
        private const int UNUSED_MASK = 0b1000000000000000;
        private int _internalValue = 0;

        public int CoarseX
        {
            get { return _internalValue & COARSE_X_MASK; }
            set
            {
                _internalValue = (_internalValue & ~COARSE_X_MASK) | (value & COARSE_X_MASK);
            }
        }

        public int CoarseY
        {
            get { return (_internalValue & COARSE_Y_MASK) >> 5; }
            set
            {
                _internalValue = (_internalValue & ~COARSE_Y_MASK) | ((value << 5) & COARSE_Y_MASK);
            }
        }

        public int NameTableX
        {
            get { return (_internalValue & NAMETABLE_X_MASK) >> 10; }
            set
            {
                _internalValue = (_internalValue & ~NAMETABLE_X_MASK) | ((value << 10) & NAMETABLE_X_MASK);
            }
        }

        public int NameTableY
        {
            get { return (_internalValue & NAMETABLE_Y_MASK) >> 11; }
            set
            {
                _internalValue = (_internalValue & ~NAMETABLE_Y_MASK) | ((value << 11) & NAMETABLE_Y_MASK);
            }
        }

        public int FineY
        {
            get { return (_internalValue & FINE_Y_MASK) >> 12; }
            set
            {
                _internalValue = (_internalValue & ~FINE_Y_MASK) | ((value << 12) & FINE_Y_MASK);
            }
        }

        public int Value
        {
            get { return _internalValue; }
            set { _internalValue = value; }
        }
    }
}
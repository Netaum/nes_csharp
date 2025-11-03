using Emulator.Components.Interfaces;
using Emulator.Components.AddressingModes.Implementations;

namespace Emulator.Components.AddressingModes
{
    public static class InstructionAddressingModes
    {
        public static readonly IAddressingMode Implied = new ImpliedAddressingMode();
        public static readonly IAddressingMode Immediate = new ImmediateAddressingMode();
        public static readonly IAddressingMode ZeroPage = new ZeroPageAddressingMode();
        public static readonly IAddressingMode ZeroPageX = new ZeroPageXAddressingMode();
        public static readonly IAddressingMode ZeroPageY = new ZeroPageYAddressingMode();
        public static readonly IAddressingMode Relative = new RelativeAddressingMode();
        public static readonly IAddressingMode Absolute = new AbsoluteAddressingMode();
        public static readonly IAddressingMode AbsoluteX = new AbsoluteXAddressingMode();
        public static readonly IAddressingMode AbsoluteY = new AbsoluteYAddressingMode();
        public static readonly IAddressingMode Indirect = new IndirectAddressingMode();
        public static readonly IAddressingMode IndirectX = new IndirectXAddressingMode();
        public static readonly IAddressingMode IndirectY = new IndirectYAddressingMode();
    }
}
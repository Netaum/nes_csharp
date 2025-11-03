using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class NOP : OpCodeBase
    {
        public override string Name => "NOP";
        public override string Description => "No Operation - Causes no changes to the processor other than the normal incrementing of the program counter";

        public override int Execute(ICpu cpu)
        {
            switch (cpu.OpCode)
            {
                case 0x1C: // NOP (SLO)
                case 0x3C: // NOP (ANC)
                case 0x5C: // NOP (SLO)
                case 0x7C: // NOP (ANC)
                case 0xDC: // NOP (SLO)
                case 0xFC: // NOP (ANC)
                    return 1; // These are not true NOPs, they have side effects.
            }
            return 0;
        }
    }
}
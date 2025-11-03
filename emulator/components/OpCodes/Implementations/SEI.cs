using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class SEI : OpCodeBase
    {
        public override string Name => "SEI";
        public override string Description => "Set Interrupt Disable - Sets the interrupt disable flag to one";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.I, true);
            return 0;
        }
    }
}
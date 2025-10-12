using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class CLI : OpCodeBase
    {
        public override string Name => "CLI";
        public override string Description => "Clear Interrupt Disable - Clears the interrupt disable flag to zero";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.I, false);
            return 0;
        }
    }
}
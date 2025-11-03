using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class CLV : OpCodeBase
    {
        public override string Name => "CLV";
        public override string Description => "Clear Overflow Flag - Clears the overflow flag to zero";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.V, false);
            return 0;
        }
    }
}
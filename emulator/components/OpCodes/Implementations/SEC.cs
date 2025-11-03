using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class SEC : OpCodeBase
    {
        public override string Name => "SEC";
        public override string Description => "Set Carry Flag - Sets the carry flag to one";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.C, true);
            return 0;
        }
    }
}
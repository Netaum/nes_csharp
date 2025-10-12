using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class BVS : OpCodeBase
    {
        public override string Name => "BVS";
        public override string Description => "Branch if Overflow Set - Branch if the overflow flag is set (V = 1)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.V);

            if (!flag)
                return 0;

            cpu.Cycles++;
            int absoluteAddress = cpu.ProgramCounter + cpu.RelativeAddress;
            cpu.AbsoluteAddress =  absoluteAddress;
            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.Cycles++;
            }

            cpu.ProgramCounter = absoluteAddress;
            return 0;
        }
    }
}
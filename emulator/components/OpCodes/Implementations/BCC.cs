using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class BCC : OpCodeBase
    {
        public override string Name => "BCC";
        public override string Description => "Branch if Carry Clear - Branch if the carry flag is clear (C = 0)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.C);

            if (flag)
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
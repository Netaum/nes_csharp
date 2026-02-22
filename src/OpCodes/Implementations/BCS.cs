using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class BCS : OpCodeBase
    {
        public override string Name => "BCS";
        public override string Description => "Branch if Carry Set - Branch if the carry flag is set (C = 1)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.C);

            if (!flag)
                return 0;

            cpu.IncreaseCycles();
            int absoluteAddress = cpu.ProgramCounter + cpu.RelativeAddress;
            cpu.SetAbsoluteAddress(absoluteAddress);
            if ((absoluteAddress & 0xFF00) != (cpu.ProgramCounter & 0xFF00))
            {
                cpu.IncreaseCycles();
            }

            cpu.SetProgramCounter(absoluteAddress);
            return 0;
        }
    }
}

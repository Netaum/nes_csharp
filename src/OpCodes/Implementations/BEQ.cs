using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class BEQ : OpCodeBase
    {
        public override string Name => "BEQ";
        public override string Description => "Branch if Equal - Branch if the zero flag is set (Z = 1)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.Z);

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

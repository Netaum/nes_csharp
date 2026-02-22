using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class BVC : OpCodeBase
    {
        public override string Name => "BVC";
        public override string Description => "Branch if Overflow Clear - Branch if the overflow flag is clear (V = 0)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.V);

            if (flag)
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

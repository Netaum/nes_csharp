using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class BPL : OpCodeBase
    {
        public override string Name => "BPL";
        public override string Description => "Branch if Positive - Branch if the negative flag is clear (N = 0)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.N);

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

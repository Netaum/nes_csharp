using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class BMI : OpCodeBase
    {
        public override string Name => "BMI";
        public override string Description => "Branch if Minus - Branch if the negative flag is set (N = 1)";

        public override int Execute(ICpu cpu)
        {
            bool flag = cpu.GetStatusFlag(Flags6502.N);

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

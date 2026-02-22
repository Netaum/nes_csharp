using Interfaces;

namespace AddressingModes.Implementations
{
    public class ImmediateAddressingMode : AddressingModeBase
    {
        public override string Name => "IMM";

        public override int Execute(ICpu cpu)
        {
            cpu.SetAbsoluteAddress(cpu.ProgramCounter);
            cpu.StepProgramCounter();
            return 0;
        }
    }
}
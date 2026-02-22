using Emulator.Components.Interfaces;

namespace Emulator.Components.AddressingModes.Implementations
{
    public class ImmediateAddressingMode : AddressingModeBase
    {
        public override string Name => "IMM";

        public override int Execute(ICpu cpu)
        {
            cpu.AbsoluteAddress = cpu.ProgramCounter;
            cpu.StepProgramCounter();
            return 0;
        }
    }
}
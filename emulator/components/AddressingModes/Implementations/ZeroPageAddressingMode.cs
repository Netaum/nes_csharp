using emulator.components.Interfaces;

namespace emulator.components.AddressingModes.Implementations
{
    public class ZeroPageAddressingMode : AddressingModeBase
    {
        public override string Name => "ZP0";

        public override int Execute(ICpu cpu)
        {
            int address = cpu.ReadMemory();
            cpu.StepProgramCounter();
            cpu.AbsoluteAddress = address & 0x00FF;
            return 0;
        }
    }
}
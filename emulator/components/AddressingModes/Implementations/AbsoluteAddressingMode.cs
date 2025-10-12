using emulator.components.Interfaces;

namespace emulator.components.AddressingModes.Implementations
{
    public class AbsoluteAddressingMode : AddressingModeBase
    {
        public override string Name => "ABS";

        public override int Execute(ICpu cpu)
        {
            int lowByte = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int highByte = cpu.ReadMemory();
            cpu.StepProgramCounter();

            int address = (highByte << 8) | lowByte;

            cpu.AbsoluteAddress = address;
            return 0;
        }
    }
}
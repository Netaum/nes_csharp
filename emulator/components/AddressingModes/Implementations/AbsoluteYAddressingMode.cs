using emulator.components.Interfaces;

namespace emulator.components.AddressingModes.Implementations
{
    public class AbsoluteYAddressingMode : AddressingModeBase
    {
        public override string Name => "ABY";

        public override int Execute(ICpu cpu)
        {
            int lowByte = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int highByte = cpu.ReadMemory();
            cpu.StepProgramCounter();

            int address = (highByte << 8) | lowByte;
            address += cpu.YRegister;

            cpu.AbsoluteAddress = address;

            if ((cpu.AbsoluteAddress & 0xFF00) != (highByte << 8))
            {
                return 1;
            }

            return 0;
        }
    }
}
using emulator.components.Interfaces;

namespace emulator.components.AddressingModes.Implementations
{
    public class IndirectYAddressingMode : AddressingModeBase
    {
        public override string Name => "IZY";

        public override int Execute(ICpu cpu)
        {
            int pointer = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int lowByte = cpu.ReadMemory(pointer & 0x00FF);
            int highByte = cpu.ReadMemory((pointer + 1) & 0x00FF);
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
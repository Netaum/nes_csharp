using Emulator.Components.Interfaces;

namespace Emulator.Components.AddressingModes.Implementations
{
    public class AbsoluteXAddressingMode : AddressingModeBase
    {
        public override string Name => "ABX";

        public override int Execute(ICpu cpu)
        {
            int lowByte = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int highByte = cpu.ReadMemory();
            cpu.StepProgramCounter();

            int address = (highByte << 8) | lowByte;
            address += cpu.XRegister;

            cpu.AbsoluteAddress = address;

            if ((cpu.AbsoluteAddress & 0xFF00) != (highByte << 8))
            {
                return 1;
            }

            return 0;
        }
    }
}
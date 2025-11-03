using Emulator.Components.Interfaces;

namespace Emulator.Components.AddressingModes.Implementations
{
    public class ZeroPageYAddressingMode : AddressingModeBase
    {
        public override string Name => "ZPY";

        public override int Execute(ICpu cpu)
        {
            int address = cpu.ReadMemory();
            cpu.StepProgramCounter();
            address += cpu.YRegister;
            cpu.AbsoluteAddress = address & 0x00FF;
            return 0;
        }
    }
}
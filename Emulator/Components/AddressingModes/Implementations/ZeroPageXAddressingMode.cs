using Emulator.Components.Interfaces;

namespace Emulator.Components.AddressingModes.Implementations
{
    public class ZeroPageXAddressingMode : AddressingModeBase
    {
        public override string Name => "ZPX";

        public override int Execute(ICpu cpu)
        {
            int address = cpu.ReadMemory();
            cpu.StepProgramCounter();
            address += cpu.XRegister;
            cpu.AbsoluteAddress = address & 0x00FF;
            return 0;
        }
    }
}
using Interfaces;

namespace AddressingModes.Implementations
{
    public class ZeroPageXAddressingMode : AddressingModeBase
    {
        public override string Name => "ZPX";

        public override int Execute(ICpu cpu)
        {
            int address = cpu.ReadMemory();
            cpu.StepProgramCounter();
            address += cpu.XRegister;
            cpu.SetAbsoluteAddress(address & 0x00FF);
            return 0;
        }
    }
}
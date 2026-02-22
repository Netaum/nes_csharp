using Interfaces;

namespace AddressingModes.Implementations
{
    public class ZeroPageAddressingMode : AddressingModeBase
    {
        public override string Name => "ZP0";

        public override int Execute(ICpu cpu)
        {
            int address = cpu.ReadMemory();
            cpu.StepProgramCounter();
            cpu.SetAbsoluteAddress(address & 0x00FF);
            return 0;
        }
    }
}
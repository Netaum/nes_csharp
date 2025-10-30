using emulator.components.Interfaces;

namespace emulator.components.AddressingModes.Implementations
{
    public class RelativeAddressingMode : AddressingModeBase
    {
        public override string Name => "REL";

        public override int Execute(ICpu cpu)
        {
            int offset = cpu.ReadMemory();
            cpu.StepProgramCounter();

            if (offset >= 0x80)
            {
                offset -= 0x100;
            }

            cpu.RelativeAddress = offset;
            return 0;
        }
    }
}
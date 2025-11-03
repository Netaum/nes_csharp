using emulator.components.Interfaces;

namespace emulator.components.AddressingModes.Implementations
{
    public class IndirectXAddressingMode : AddressingModeBase
    {
        public override string Name => "IZX";

        public override int Execute(ICpu cpu)
        {
            int pointer = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int lowByte = cpu.ReadMemory((pointer + cpu.XRegister) & 0x00FF);
            int highByte = cpu.ReadMemory((pointer + cpu.XRegister + 1) & 0x00FF);
            cpu.AbsoluteAddress = (highByte << 8) | lowByte;
            return 0;
        }
    }
}
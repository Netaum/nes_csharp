using Emulator.Components.Interfaces;

namespace Emulator.Components.AddressingModes.Implementations
{
    public class IndirectAddressingMode : AddressingModeBase
    {
        public override string Name => "IND";

        public override int Execute(ICpu cpu)
        {
            int ptrLow = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int ptrHigh = cpu.ReadMemory();
            cpu.StepProgramCounter();
            int pointer = (ptrHigh << 8) | ptrLow;
            
            int lowByte = cpu.ReadMemory(pointer);
            int highByte = ptrLow == 0x00FF ?
                                cpu.ReadMemory(pointer & 0xFF00) : 
                                cpu.ReadMemory(pointer + 1);

            cpu.AbsoluteAddress = (highByte << 8) | lowByte;

            return 0;
        }
    }
}
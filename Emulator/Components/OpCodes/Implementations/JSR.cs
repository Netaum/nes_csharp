using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class JSR : OpCodeBase
    {
        public override string Name => "JSR";
        public override string Description => "Jump to Subroutine - Pushes the return address onto the stack and jumps to the target address";

        public override int Execute(ICpu cpu)
        {
            cpu.ProgramCounter--;
            cpu.WriteMemory(0x0100 + cpu.StackPointer, (cpu.ProgramCounter >> 8).ToByte());
            cpu.StackPointer--;
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.ProgramCounter.ToByte());
            cpu.StackPointer--;
            cpu.ProgramCounter = cpu.AbsoluteAddress;
            return 0;
        }
    }
}
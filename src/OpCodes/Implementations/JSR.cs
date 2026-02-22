using Helpers;
using Interfaces;

namespace OpCodes.Implementations
{
    public class JSR : OpCodeBase
    {
        public override string Name => "JSR";
        public override string Description => "Jump to Subroutine - Pushes the return address onto the stack and jumps to the target address";

        public override int Execute(ICpu cpu)
        {
            cpu.StepProgramCounter(-1);
            cpu.WriteMemory(0x0100 + cpu.StackPointer, (cpu.ProgramCounter >> 8).ToByte());
            cpu.DecreaseStackPointer();
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.ProgramCounter.ToByte());
            cpu.DecreaseStackPointer();
            cpu.SetProgramCounter(cpu.AbsoluteAddress);
            return 0;
        }
    }
}

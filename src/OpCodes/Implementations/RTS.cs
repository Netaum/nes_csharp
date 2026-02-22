using Interfaces;

namespace OpCodes.Implementations
{
    public class RTS : OpCodeBase
    {
        public override string Name => "RTS";
        public override string Description => "Return from Subroutine - Returns from a subroutine by pulling the program counter from the stack";

        public override int Execute(ICpu cpu)
        {
            cpu.IncreaseStackPointer();
            int low = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.IncreaseStackPointer();
            int high = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.SetProgramCounter(low | (high << 8));
            cpu.SetProgramCounter(cpu.ProgramCounter + 1);
            return 0;
        }
    }
}

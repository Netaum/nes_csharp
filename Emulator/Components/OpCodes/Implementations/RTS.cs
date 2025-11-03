using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class RTS : OpCodeBase
    {
        public override string Name => "RTS";
        public override string Description => "Return from Subroutine - Returns from a subroutine by pulling the program counter from the stack";

        public override int Execute(ICpu cpu)
        {
            cpu.StackPointer++;
            int low = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.StackPointer++;
            int high = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.ProgramCounter = low | (high << 8);
            cpu.ProgramCounter++;
            return 0;
        }
    }
}
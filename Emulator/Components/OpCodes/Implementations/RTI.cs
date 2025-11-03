using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class RTI : OpCodeBase
    {
        public override string Name => "RTI";
        public override string Description => "Return from Interrupt - Returns from an interrupt by pulling the processor flags and program counter from the stack";

        public override int Execute(ICpu cpu)
        {
            cpu.StackPointer++;
            cpu.Status = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.Status &= ~(int)Flags6502.B;
            cpu.Status &= ~(int)Flags6502.U;

            cpu.StackPointer++;
            int low = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.StackPointer++;
            int high = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.ProgramCounter = low | (high << 8);
            return 0;
        }
    }
}
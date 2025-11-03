using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class PHP : OpCodeBase
    {
        public override string Name => "PHP";
        public override string Description => "Push Processor Status - Pushes a copy of the status flags on to the stack";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(0x0100 + cpu.StackPointer, (cpu.Status | (int)Flags6502.B | (int)Flags6502.U).ToByte());
            cpu.SetStatusFlag(Flags6502.B, false);
            cpu.SetStatusFlag(Flags6502.U, false);
            cpu.StackPointer--;
            return 0;
        }
    }
}
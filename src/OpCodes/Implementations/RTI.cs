using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class RTI : OpCodeBase
    {
        public override string Name => "RTI";
        public override string Description => "Return from Interrupt - Returns from an interrupt by pulling the processor flags and program counter from the stack";

        public override int Execute(ICpu cpu)
        {
            cpu.IncreaseStackPointer();
            cpu.SetStatus(cpu.ReadMemory(0x0100 + cpu.StackPointer));
            cpu.SetStatusFlag(Flags6502.B, false);
            cpu.SetStatusFlag(Flags6502.U, false);

            cpu.IncreaseStackPointer();
            int low = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.IncreaseStackPointer();
            int high = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.SetProgramCounter(low | (high << 8));
            return 0;
        }
    }
}

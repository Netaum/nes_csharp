using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class PLA : OpCodeBase
    {
        public override string Name => "PLA";
        public override string Description => "Pull Accumulator - Pulls an 8-bit value from the stack and into the accumulator";

        public override int Execute(ICpu cpu)
        {
            cpu.StackPointer++;
            cpu.AccumulatorRegister = cpu.ReadMemory(0x0100 + cpu.StackPointer);
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 0;
        }
    }
}
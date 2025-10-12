using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class LDA : OpCodeBase
    {
        public override string Name => "LDA";
        public override string Description => "Load Accumulator - Loads a byte of memory into the accumulator";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.AccumulatorRegister = fetchedValue;
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 1;
        }
    }
}
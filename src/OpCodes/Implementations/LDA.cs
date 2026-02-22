using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class LDA : OpCodeBase
    {
        public override string Name => "LDA";
        public override string Description => "Load Accumulator - Loads a byte of memory into the accumulator";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.SetAccumulatorRegister(fetchedValue);
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 1;
        }
    }
}

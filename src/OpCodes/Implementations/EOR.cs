using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class EOR : OpCodeBase
    {
        public override string Name => "EOR";
        public override string Description => "Exclusive OR - Performs a logical exclusive-OR with the accumulator";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.SetAccumulatorRegister(cpu.AccumulatorRegister ^ fetchedValue);
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 1;
        }
    }
}

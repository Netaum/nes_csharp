using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class ORA : OpCodeBase
    {
        public override string Name => "ORA";
        public override string Description => "Logical Inclusive OR - Performs a logical OR with the accumulator";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.SetAccumulatorRegister(cpu.AccumulatorRegister | fetchedValue);
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 1;
        }
    }
}

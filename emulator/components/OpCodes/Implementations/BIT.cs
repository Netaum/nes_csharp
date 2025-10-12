using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class BIT : OpCodeBase
    {
        public override string Name { get; } = "BIT";
        public override string Description { get; } = "Bit Test - Tests if one or more bits are set in a target memory location";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = cpu.AccumulatorRegister & fetchedValue;
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (fetchedValue & (1 << 7)) != 0);
            cpu.SetStatusFlag(Flags6502.V, (fetchedValue & (1 << 6)) != 0);
            return 0;
        }
    }
}
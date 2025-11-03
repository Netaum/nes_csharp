using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class CMP : OpCodeBase
    {
        public override string Name => "CMP";
        public override string Description => "Compare - Compares the contents of the accumulator with another memory value";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = cpu.AccumulatorRegister - fetchedValue;
            cpu.SetStatusFlag(Flags6502.C, cpu.AccumulatorRegister >= fetchedValue);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 1;
        }
    }
}
using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class ADC : OpCodeBase
    {
        public override string Name => "ADC";
        public override string Description => "Add with Carry - Adds the value at the memory location to the accumulator with carry";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = cpu.AccumulatorRegister + fetchedValue + (cpu.GetStatusFlag(Flags6502.C) ? 1 : 0);
            cpu.SetStatusFlag(Flags6502.C, temp > 0xFF);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            cpu.SetStatusFlag(Flags6502.V, (~(cpu.AccumulatorRegister ^ fetchedValue) & (cpu.AccumulatorRegister ^ temp) & 0x80) == 0x80);
            cpu.AccumulatorRegister = temp & 0x00FF;
            return 1;
        }
    }
}
using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class AND : OpCodeBase
    {
        public override string Name => "AND";
        public override string Description => "Logical AND - Performs a bitwise AND on the accumulator and the value at the memory location";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.AccumulatorRegister = cpu.AccumulatorRegister & fetchedValue;
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 1;
        }
    }
}
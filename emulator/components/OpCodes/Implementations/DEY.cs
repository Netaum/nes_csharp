using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class DEY : OpCodeBase
    {
        public override string Name => "DEY";
        public override string Description => "Decrement Y Register - Subtracts one from the Y register";

        public override int Execute(ICpu cpu)
        {
            cpu.YRegister = (cpu.YRegister - 1) & 0x00FF;
            cpu.SetStatusFlag(Flags6502.Z, cpu.YRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.YRegister & 0x80) == 0x80);
            return 0;
        }
    }
}
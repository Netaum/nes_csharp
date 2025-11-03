using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class DEX : OpCodeBase
    {
        public override string Name => "DEX";
        public override string Description => "Decrement X Register - Subtracts one from the X register";

        public override int Execute(ICpu cpu)
        {
            cpu.XRegister = (cpu.XRegister - 1) & 0x00FF;
            cpu.SetStatusFlag(Flags6502.Z, cpu.XRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.XRegister & 0x80) == 0x80);
            return 0;
        }
    }
}
using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class LDX : OpCodeBase
    {
        public override string Name => "LDX";
        public override string Description => "Load X Register - Loads a byte of memory into the X register";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            cpu.XRegister = fetchedValue;
            cpu.SetStatusFlag(Flags6502.Z, cpu.XRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.XRegister & 0x80) == 0x80);
            return 1;
        }
    }
}
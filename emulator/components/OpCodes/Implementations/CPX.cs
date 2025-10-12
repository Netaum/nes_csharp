using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class CPX : OpCodeBase
    {
        public override string Name => "CPX";
        public override string Description => "Compare X Register - Compares the contents of the X register with another memory value";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = cpu.XRegister - fetchedValue;
            cpu.SetStatusFlag(Flags6502.C, cpu.XRegister >= fetchedValue);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 1;
        }
    }
}
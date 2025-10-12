using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class INC : OpCodeBase
    {
        public override string Name => "INC";
        public override string Description => "Increment Memory - Adds one to the value held at a specified memory location";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = fetchedValue + 1;
            cpu.WriteMemory(cpu.AbsoluteAddress, temp.ToByte());
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);
            return 0;
        }
    }
}
using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class STY : OpCodeBase
    {
        public override string Name => "STY";
        public override string Description => "Store Y Register - Stores the contents of the Y register into memory";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(cpu.AbsoluteAddress, cpu.YRegister.ToByte());
            return 0;
        }
    }
}
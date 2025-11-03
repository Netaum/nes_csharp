using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class STX : OpCodeBase
    {
        public override string Name => "STX";
        public override string Description => "Store X Register - Stores the contents of the X register into memory";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(cpu.AbsoluteAddress, cpu.XRegister.ToByte());
            return 0;
        }
    }
}
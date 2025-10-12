using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class TXS : OpCodeBase
    {
        public override string Name => "TXS";
        public override string Description => "Transfer X to Stack Pointer - Copies the current contents of the X register into the stack pointer";

        public override int Execute(ICpu cpu)
        {
            cpu.StackPointer = cpu.XRegister;
            return 0;
        }
    }
}
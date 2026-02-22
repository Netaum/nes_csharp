using Helpers;
using Interfaces;

namespace OpCodes.Implementations
{
    public class TXS : OpCodeBase
    {
        public override string Name => "TXS";
        public override string Description => "Transfer X to Stack Pointer - Copies the current contents of the X register into the stack pointer";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStackPointer(cpu.XRegister);
            return 0;
        }
    }
}
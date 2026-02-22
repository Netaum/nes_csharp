using Helpers;
using Interfaces;

namespace OpCodes.Implementations
{
    public class STA : OpCodeBase
    {
        public override string Name => "STA";
        public override string Description => "Store Accumulator - Stores the contents of the accumulator into memory";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(cpu.AbsoluteAddress, cpu.AccumulatorRegister.ToByte());
            return 0;
        }
    }
}

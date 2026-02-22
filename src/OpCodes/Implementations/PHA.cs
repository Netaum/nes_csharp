using Helpers;
using Interfaces;

namespace OpCodes.Implementations
{
    public class PHA : OpCodeBase
    {
        public override string Name => "PHA";
        public override string Description => "Push Accumulator - Pushes a copy of the accumulator on to the stack";

        public override int Execute(ICpu cpu)
        {
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.AccumulatorRegister.ToByte());
            cpu.DecreaseStackPointer();
            return 0;
        }
    }
}

using Helpers;
using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class TAY : OpCodeBase
    {
        public override string Name => "TAY";
        public override string Description => "Transfer Accumulator to Y - Copies the current contents of the accumulator into the Y register";

        public override int Execute(ICpu cpu)
        {
            cpu.SetYRegister(cpu.AccumulatorRegister);
            cpu.SetStatusFlag(Flags6502.Z, cpu.YRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.YRegister & 0x80) == 0x80);
            return 0;
        }
    }
}
using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class TAX : OpCodeBase
    {
        public override string Name => "TAX";
        public override string Description => "Transfer Accumulator to X - Copies the current contents of the accumulator into the X register";

        public override int Execute(ICpu cpu)
        {
            cpu.SetXRegister(cpu.AccumulatorRegister);
            cpu.SetStatusFlag(Flags6502.Z, cpu.XRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.XRegister & 0x80) == 0x80);
            return 0;
        }
    }
}
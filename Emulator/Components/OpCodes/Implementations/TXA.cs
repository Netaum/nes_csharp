using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class TXA : OpCodeBase
    {
        public override string Name => "TXA";
        public override string Description => "Transfer X to Accumulator - Copies the current contents of the X register into the accumulator";

        public override int Execute(ICpu cpu)
        {
            cpu.AccumulatorRegister = cpu.XRegister;
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 0;
        }
    }
}
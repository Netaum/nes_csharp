using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class TYA : OpCodeBase
    {
        public override string Name => "TYA";
        public override string Description => "Transfer Y to Accumulator - Copies the current contents of the Y register into the accumulator";

        public override int Execute(ICpu cpu)
        {
            cpu.AccumulatorRegister = cpu.YRegister;
            cpu.SetStatusFlag(Flags6502.Z, cpu.AccumulatorRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.AccumulatorRegister & 0x80) == 0x80);
            return 0;
        }
    }
}
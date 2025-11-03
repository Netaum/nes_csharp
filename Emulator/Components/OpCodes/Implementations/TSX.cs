using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class TSX : OpCodeBase
    {
        public override string Name => "TSX";
        public override string Description => "Transfer Stack Pointer to X - Copies the current contents of the stack pointer into the X register";

        public override int Execute(ICpu cpu)
        {
            cpu.XRegister = cpu.StackPointer;
            cpu.SetStatusFlag(Flags6502.Z, cpu.XRegister == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (cpu.XRegister & 0x80) == 0x80);
            return 0;
        }
    }
}
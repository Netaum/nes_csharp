using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class CLC : OpCodeBase
    {
        public override string Name => "CLC";
        public override string Description => "Clear Carry Flag - Clears the carry flag to zero";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.C, false);
            return 0;
        }
    }
}
using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class CLD : OpCodeBase
    {
        public override string Name => "CLD";
        public override string Description => "Clear Decimal Mode - Clears the decimal mode flag to zero";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.D, false);
            return 0;
        }
    }
}
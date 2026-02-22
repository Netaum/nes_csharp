using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class SED : OpCodeBase
    {
        public override string Name => "SED";
        public override string Description => "Set Decimal Flag - Sets the decimal mode flag to one";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.D, true);
            return 0;
        }
    }
}
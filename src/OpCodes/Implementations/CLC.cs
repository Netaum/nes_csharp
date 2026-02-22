using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
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

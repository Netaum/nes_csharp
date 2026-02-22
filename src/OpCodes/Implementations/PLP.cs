using Interfaces;
using Interfaces.Enums;

namespace OpCodes.Implementations
{
    public class PLP : OpCodeBase
    {
        public override string Name => "PLP";
        public override string Description => "Pull Processor Status - Pulls an 8-bit value from the stack and into the processor flags";

        public override int Execute(ICpu cpu)
        {
            cpu.IncreaseStackPointer();
            cpu.SetStatus(cpu.ReadMemory(0x0100 + cpu.StackPointer));
            cpu.SetStatusFlag(Flags6502.U, true);
            return 0;
        }
    }
}

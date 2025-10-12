using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class XXX : OpCodeBase
    {
        public override string Name => "XXX";
        public override string Description => "Illegal Opcode - This is an illegal or undefined opcode that performs no operation";

        public override int Execute(ICpu _cpu)
        {
            // This is an illegal opcode. It does nothing.
            return 0;
        }
    }
}
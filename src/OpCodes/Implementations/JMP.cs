using Interfaces;

namespace OpCodes.Implementations
{
    public class JMP : OpCodeBase
    {
        public override string Name => "JMP";
        public override string Description => "Jump - Sets the program counter to the address specified by the operand";

        public override int Execute(ICpu cpu)
        {
            cpu.SetProgramCounter(cpu.AbsoluteAddress);
            return 0;
        }
    }
}

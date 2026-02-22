using Interfaces;

namespace AddressingModes.Implementations
{
    public class ImpliedAddressingMode : AddressingModeBase
    {
        public override string Name => "IMP";

        public override int Execute(ICpu cpu)
        {
            cpu.SetFetchedValue(cpu.AccumulatorRegister);
            return 0;
        }
    }
}
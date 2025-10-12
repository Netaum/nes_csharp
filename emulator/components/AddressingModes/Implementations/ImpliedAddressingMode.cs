using emulator.components.Interfaces;
using emulator.components.AddressingModes.Implementations;

namespace emulator.components.AddressingModes.Implementations
{
    public class ImpliedAddressingMode : AddressingModeBase
    {
        public override string Name => "IMP";

        public override int Execute(ICpu cpu)
        {
            cpu.FetchedValue = cpu.AccumulatorRegister;
            return 0;
        }
    }
}
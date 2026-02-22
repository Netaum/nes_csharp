using Emulator.Components.Interfaces;
using Emulator.Components.AddressingModes.Implementations;

namespace Emulator.Components.AddressingModes.Implementations
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
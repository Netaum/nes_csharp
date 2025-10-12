using emulator.components.Interfaces;

namespace emulator.components.AddressingModes.Implementations
{
    public abstract class AddressingModeBase : IAddressingMode
    {
        public abstract string Name { get; }
        public abstract int Execute(ICpu cpu);

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is AddressingModeBase mode)
            {
                return mode.Name == Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
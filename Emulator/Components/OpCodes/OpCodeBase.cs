using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes
{
    public abstract class OpCodeBase : IOpCode
    {
        public abstract string Name { get; }
        public abstract string Description { get; }

        public abstract int Execute(ICpu cpu);

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object? obj)
        {
            if (obj is IOpCode other)
            {
                return Name == other.Name;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
}
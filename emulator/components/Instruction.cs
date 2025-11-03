using emulator.components.Interfaces;

namespace emulator.components
{
    public record Instruction
    {
        public string Name { get; init; }
        public int Cycles { get; init; }
        public IAddressingMode AddressingMode { get; init; }
        public IOpCode Operation { get; init; }

        public Instruction(string name, IOpCode operation, IAddressingMode addressingMode, int cycles)
        {
            Name = name;
            Cycles = cycles;
            AddressingMode = addressingMode;
            Operation = operation;
        }
    }
}
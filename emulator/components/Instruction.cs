namespace emulator.components
{
    public record Instruction
    {
        public string Name { get; init; }
        public int Cycles { get; init; }
        public IAddressingMode AddressingMode { get; init; }
        public Func<int> Operation { get; init; }

        public Instruction(string name, Func<int> operation, IAddressingMode addressingMode, int cycles)
        {
            Name = name;
            Cycles = cycles;
            AddressingMode = addressingMode;
            Operation = operation;
        }
    }
}
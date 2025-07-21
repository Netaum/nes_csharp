namespace nes_csharp.components
{
    public record Instruction
    {
        public string Name { get; init; }
        public int Cycles { get; init; }
        public Func<int> AddressingMode { get; init; }
        public Func<int> Operation { get; init; }

        public Instruction(string name, Func<int> addressingMode, Func<int> operation, int cycles)
        {
            Name = name;
            Cycles = cycles;
            AddressingMode = addressingMode;
            Operation = operation;
        }
    }
}
namespace emulator.components.Interfaces
{
    public interface IOpCode
    {
        string Name { get; }
        string Description { get; }
        int Execute(ICpu cpu);
    }
}
namespace emulator.components.Interfaces
{
    public interface IOpCode
    {
        string Name { get; }
        int Execute(ICpu cpu);
    }
}
namespace Interfaces
{
    public interface IAddressingMode
    {
        string Name { get; }
        int Execute(ICpu cpu);
    }
}
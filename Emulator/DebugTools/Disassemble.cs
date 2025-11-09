using Emulator.Components.Interfaces;

namespace Emulator.DebugTools
{
    public static class Disassemble
    {

        public static Dictionary<int, DisassembledItem> FromMemory(ICpu cpu, int startAddress, int endAddress)
        {
            var items = new Dictionary<int, DisassembledItem>();
            int address = startAddress;
            while (address < endAddress)
            {
                var (nextAddress, item) = DisassembledItem.FromMemory(cpu, address);
                items.Add(address, item);
                address = nextAddress;
            }
            return items;
        }
    }
}
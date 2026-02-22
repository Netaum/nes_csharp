using AddressingModes;
using Chips;
using Interfaces;

namespace DebugTools;

public static class Disassemble
{
    public static Dictionary<int, string> Execute(this ICpu cpu, int start, int stop)
    {
        int address = start;
        var disassembly = new Dictionary<int, string>();

        while (address < stop)
        {
            int lineAddress = address;
            string sInst = $"${address:X4}: ";
            int opcode = cpu.ReadMemory(address);
            IInstruction instruction = Instruction.GetInstruction(opcode);

            address++;
            sInst += $"{instruction.Name} ";

            int value;
            int lowByte;
            int highByte;
            if (instruction.AddressingMode == InstructionAddressingModes.Implied)
            {
                sInst += " {IMP}";
            }
            else if (instruction.AddressingMode == InstructionAddressingModes.Immediate)
            {
                value = cpu.ReadMemory(address);
                address++;
                sInst += $"#${value:X2} {{IMM}}";
            }
            else if (instruction.AddressingMode == InstructionAddressingModes.ZeroPage)
            {
                lowByte = cpu.ReadMemory(address);
                address++;
                highByte = 0x00;
                sInst += $"${lowByte:X2} {{ZP0}}";
            }
            else if (instruction.AddressingMode == InstructionAddressingModes.ZeroPageX)
            {
                lowByte = cpu.ReadMemory(address);
                address++;
                sInst += $"${lowByte:X2},X {{ZPX}}";
            }
            else if (instruction.AddressingMode == InstructionAddressingModes.ZeroPageY)
            {
                lowByte = cpu.ReadMemory(address);
                address++;
                highByte = 0x00;
                sInst += $"${lowByte:X2},Y {{ZPY}}";
            }
            else if (instruction.AddressingMode == InstructionAddressingModes.IndirectX)
            {
                lowByte = cpu.ReadMemory(address);
                address++;
                highByte = 0x00;
                sInst += $"(${lowByte:X2},X) {{IZX}}";
            }
            else if (instruction.AddressingMode == InstructionAddressingModes.IndirectY)
            {
                lowByte = cpu.ReadMemory(address);
                address++;
                highByte = 0x00;
                sInst += $"(${lowByte:X2},) Y {{IZY}}";
            }

            else if (instruction.AddressingMode == InstructionAddressingModes.Absolute)
            {
                lowByte = cpu.ReadMemory(address);
                address++;
                highByte = cpu.ReadMemory(address);
                address++;
                sInst += $"${highByte << 8 | lowByte:X4} {{ABS}}";
            }
            else if (instruction.AddressingMode == InstructionAddressingModes.AbsoluteX)
            {
                lowByte = cpu.ReadMemory(address);
                address++;
                highByte = cpu.ReadMemory(address);
                address++;
                sInst += $"${highByte << 8 | lowByte:X4}, X {{ABX}}";
            }
            else if (instruction.AddressingMode == InstructionAddressingModes.AbsoluteY)
            {
                lowByte = cpu.ReadMemory(address);
                address++;
                highByte = cpu.ReadMemory(address);
                address++;
                sInst += $"${highByte << 8 | lowByte:X4}, Y {{ABY}}";
            }
            else if (instruction.AddressingMode == InstructionAddressingModes.Indirect)
            {
                lowByte = cpu.ReadMemory(address);
                address++;
                highByte = cpu.ReadMemory(address);
                address++;
                sInst += $"(${highByte << 8 | lowByte:X4}) {{IND}}";
            }
            else if (instruction.AddressingMode == InstructionAddressingModes.Relative)
            {
                value = cpu.ReadMemory(address);
                address++;
                sInst += $"${value:X2} [${address + value:X4}] {{REL}}";
            }

            disassembly[lineAddress] = sInst;
        }

        return disassembly;
    }
}

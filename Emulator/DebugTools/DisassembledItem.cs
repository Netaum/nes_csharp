using System.Text;
using Emulator.Components;
using Emulator.Components.AddressingModes;
using Emulator.Components.Interfaces;
using Emulator.Components.OpCodes;

namespace Emulator.DebugTools
{
    public class DisassembledItem
    {
        private static IAddressingMode[] _directAddressing =
        [
            InstructionAddressingModes.Immediate,
            InstructionAddressingModes.ZeroPage,
            InstructionAddressingModes.ZeroPageX,
            InstructionAddressingModes.ZeroPageY,
            InstructionAddressingModes.IndirectX,
            InstructionAddressingModes.IndirectY,
            InstructionAddressingModes.Relative
        ];

        private static IAddressingMode[] _indirectAddressing =
        [
            InstructionAddressingModes.Indirect,
            InstructionAddressingModes.Absolute,
            InstructionAddressingModes.AbsoluteX,
            InstructionAddressingModes.AbsoluteY
        ];

        private static Instruction _noopInstruction = new Instruction("NOP", ProcessorInstructions.NOP, InstructionAddressingModes.Implied, 2);
        public Instruction Instruction { get; private set; } = _noopInstruction;
        public int Address { get; private set; } = 0;
        public int Value { get; private set; } = 0;

        private DisassembledItem() { }

        public static (int, DisassembledItem) FromMemory(ICpu cpu, int address)
        {
            var item = new DisassembledItem();
            var Instruction = cpu.GetInstruction(address);
            item.Instruction = Instruction;
            item.Address = address;

            address++;
            int value = 0x0000;

            if (_directAddressing.Contains(Instruction.AddressingMode))
            {
                value = cpu.ReadMemory(address);
                address++;
            }
            else if (_indirectAddressing.Contains(Instruction.AddressingMode))
            {
                int lowByte = cpu.ReadMemory(address);
                address++;
                int highByte = cpu.ReadMemory(address);
                address++;
                value = (highByte << 8) | lowByte;
            }
            item.Value = value;
            return (address, item);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("{0:X4}: {1} ", Address, Instruction.Name);

            if (Instruction.AddressingMode == InstructionAddressingModes.Implied)
            {
                sb.Append("{IMP}");
            }
            else if (Instruction.AddressingMode == InstructionAddressingModes.Immediate)
            {
                sb.AppendFormat("#${0:X2} {{IMM}}", Value);
            }
            else if (Instruction.AddressingMode == InstructionAddressingModes.ZeroPage)
            {
                sb.AppendFormat("#${0:X2} {{ZP0}}", Value);
            }
            else if (Instruction.AddressingMode == InstructionAddressingModes.ZeroPageX)
            {
                sb.AppendFormat("${0:X2},X {{ZPX}}", Value);
            }
            else if (Instruction.AddressingMode == InstructionAddressingModes.ZeroPageY)
            {
                sb.AppendFormat("${0:X2},Y {{ZPY}}", Value);
            }
            else if (Instruction.AddressingMode == InstructionAddressingModes.IndirectX)
            {
                sb.AppendFormat("(${0:X2},X) {{IZX}}", Value);
            }
            else if (Instruction.AddressingMode == InstructionAddressingModes.IndirectY)
            {
                sb.AppendFormat("(${0:X2},Y) {{IZY}}", Value);
            }
            else if (Instruction.AddressingMode == InstructionAddressingModes.Absolute)
            {
                sb.AppendFormat("${0:X4} {{ABS}}", Value);
            }
            else if (Instruction.AddressingMode == InstructionAddressingModes.AbsoluteX)
            {
                sb.AppendFormat("${0:X4}, X {{ABX}}", Value);
            }
            else if (Instruction.AddressingMode == InstructionAddressingModes.AbsoluteY)
            {
                sb.AppendFormat("${0:X4}, Y {{ABY}}", Value);
            }
            else if (Instruction.AddressingMode == InstructionAddressingModes.Indirect)
            {
                sb.AppendFormat("(${0:X4}) {{IND}}", Value);
            }
            else if (Instruction.AddressingMode == InstructionAddressingModes.Relative)
            {
                sb.AppendFormat("${0:X2} [${1:X4}] {{REL}}", Value, Address + Value);
            }

            return sb.ToString();
        }
    }
}
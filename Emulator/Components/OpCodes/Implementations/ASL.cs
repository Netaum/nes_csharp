using Emulator.Components.AddressingModes;
using Emulator.Components.Enums;
using Emulator.Components.Interfaces;

namespace Emulator.Components.OpCodes.Implementations
{
    public class ASL : OpCodeBase
    {
        public override string Name => "ASL";
        public override string Description => "Arithmetic Shift Left - Shifts all bits of the accumulator or memory location one bit left";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = fetchedValue << 1;
            cpu.SetStatusFlag(Flags6502.C, (temp & 0xFF00) != 0);
            cpu.SetStatusFlag(Flags6502.Z, (temp & 0x00FF) == 0x00);
            cpu.SetStatusFlag(Flags6502.N, (temp & 0x80) == 0x80);

            if (cpu.CurrentInstruction.AddressingMode == InstructionAddressingModes.Implied)
            {
                cpu.AccumulatorRegister = temp & 0x00FF;
            }
            else
            {
                cpu.WriteMemory(cpu.AbsoluteAddress, temp.ToByte());
            }

            return 0;
        }
    }
}
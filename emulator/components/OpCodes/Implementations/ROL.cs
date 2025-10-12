using emulator.components.AddressingModes;
using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class ROL : OpCodeBase
    {
        public override string Name => "ROL";
        public override string Description => "Rotate Left - Rotates all bits of the accumulator or memory location one bit left";

        public override int Execute(ICpu cpu)
        {
            int fetchedValue = cpu.Fetch();
            int temp = (fetchedValue << 1) | (cpu.GetStatusFlag(Flags6502.C) ? 1 : 0);
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
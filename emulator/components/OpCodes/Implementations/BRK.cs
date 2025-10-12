using emulator.components.Enums;
using emulator.components.Interfaces;

namespace emulator.components.OpCodes.Implementations
{
    public class BRK : OpCodeBase
    {
        public override string Name => "BRK";
        public override string Description => "Force Break - Forces the generation of an interrupt request";

        public override int Execute(ICpu cpu)
        {
            cpu.SetStatusFlag(Flags6502.I, true);
            cpu.WriteMemory(0x0100 + cpu.StackPointer, ((cpu.ProgramCounter + 1) >> 8).ToByte());
            cpu.StackPointer--;
            cpu.WriteMemory(0x0100 + cpu.StackPointer, (cpu.ProgramCounter + 1).ToByte());
            cpu.SetStatusFlag(Flags6502.B, true);
            cpu.Status = (int) (Flags6502.C | Flags6502.Z | Flags6502.I | Flags6502.D | Flags6502.B | Flags6502.U | Flags6502.V | Flags6502.N);
            cpu.StackPointer--;
            cpu.WriteMemory(0x0100 + cpu.StackPointer, cpu.Status.ToByte());
            cpu.StackPointer--;
            cpu.SetStatusFlag(Flags6502.I, false);
            int low = cpu.ReadMemory(0xFFFE);
            int high = cpu.ReadMemory(0xFFFF);
            cpu.ProgramCounter = low | (high << 8);
            return 0;
        }
    }
}
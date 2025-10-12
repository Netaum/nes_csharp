using emulator.components.Enums;

namespace emulator.components.Interfaces
{
    public interface ICpu
    {
        void Clock();
        void Reset();
        void Interrupt();
        void NonMaskableInterrupt();
        byte ReadMemory(int address, bool readOnly = false);
        byte ReadMemory();
        void WriteMemory(int address, byte value);
        void LoadProgram(byte[] program, int startAddress);
        Dictionary<int, string> Disassemble(int start, int stop);
        bool GetStatusFlag(Flags6502 flag);
        void SetStatusFlag(Flags6502 flag, bool set);
        Instruction[] Instructions { get; }
        int Fetch();
        int ProgramCounter { get; set; }
        int AccumulatorRegister { get; set; }
        int XRegister { get; set; }
        int YRegister { get; set; }
        int StackPointer { get; set; }
        int AbsoluteAddress { get; set; }
        int RelativeAddress { get; set; }
        int FetchedValue { get; set; }
        int Cycles { get; set; }        
        void StepProgramCounter(int offset);
        void StepProgramCounter();
        Instruction CurrentInstruction { get; }
        int Status { get; set; }
        int OpCode { get; }
        bool Complete { get; }

    }
}
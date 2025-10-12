using emulator.components.Enums;

namespace emulator.components.Interfaces
{
    public interface ICpu
    {
        void ConnectBus(IBus bus);
        void Clock();
        void Reset();
        void Interrupt();
        void NonMaskableInterrupt();
        int ReadMemory(int address);
        int ReadMemory();
        void WriteMemory(int address, int value, bool readOnly = false);
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
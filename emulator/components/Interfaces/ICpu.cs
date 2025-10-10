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
        int ProgramCounter { get; }
        int AccumulatorRegister { get; set; }
        int XRegister { get; set; }
        int YRegister { get; set; }
        int StackPointer { get; }
        int AbsoluteAddress { get; }
        void StepProgramCounter(int offset);
        void SetProgramCounter(int address);
        void StepProgramCounter();
        void SetAbsoluteAddress(int address);
        void SetRelativeAddress(int address);
        int GetRelativeAddress();
        void SetFetchedValue(int value);
        Instruction CurrentInstruction { get; }

        void IncCycles();
        void DecCycles();

        int Status { get; set; }

        int OpCode { get; }

        void PopStackPointer();
        void PushStackPointer();

        void SetStackPointer(int value);

    }
}
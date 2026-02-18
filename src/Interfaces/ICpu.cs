using Interfaces.Enums;

namespace Interfaces
{
    public interface ICpu
    {
        void ConnectBus(IBus bus);

        //STATUS FLAG METHODS
        bool GetStatusFlag(Flags6502 flag);
        void SetStatusFlag(Flags6502 flag, bool set);
        int Status { get; }
        
        //BASIC CPU INTERFACE
        void Clock();
        void Reset();
        void Interrupt();
        void NonMaskableInterrupt();
        byte ReadMemory(int address, bool readOnly = false);
        byte ReadMemory();
        void WriteMemory(int address, byte value);
        void LoadProgram(byte[] program, int startAddress);

        //REGISTERS
        int XRegister { get; }
        int YRegister { get; }
        int AccumulatorRegister { get; }

        void SetXRegister(int value);
        void SetYRegister(int value);
        void SetAccumulatorRegister(int value); 

        //ADDRESSES
        int ProgramCounter { get; }     
        int StackPointer { get; }
        int AbsoluteAddress { get; }
        int RelativeAddress { get; }
        int FetchedValue { get; }

        void StepProgramCounter(int offset);
        void StepProgramCounter();
        void PushStack(byte value);
        byte PopStack();
        void SetAbsoluteAddress(int address);
        void SetRelativeAddress(int offset);

        //INSTRUCTION
        int Cycles { get; }        
        IInstruction CurrentInstruction { get; }
        int OpCode { get; }
        bool Complete { get; }
    }
}
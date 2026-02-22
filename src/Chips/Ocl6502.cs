using AddressingModes;
using Helpers;
using Interfaces;
using Interfaces.Enums;

namespace Chips;

public class Ocl6502 : ICpu
{
    private IBus? _bus;
    private int _fetchedData = 0x00;
    private int _absoluteAddress = 0x0000;
    private int _relativeAddress = 0x00;
    private int _programCounter = 0x0000;
    private int _stackPointer = 0x00;
    private int _accumulatorRegister = 0x00;
    private int _xRegister = 0x00;
    private int _yRegister = 0x00;
    private int _statusRegister = 0x00;
    private int _cycles = 0;
    private int _opCode = 0x00;

    private IInstruction? _currentInstruction;

    public int Status => _statusRegister;

    public int XRegister => _xRegister;

    public int YRegister => _yRegister;

    public int AccumulatorRegister => _accumulatorRegister;

    public int ProgramCounter => _programCounter;

    public int StackPointer => _stackPointer;

    public int AbsoluteAddress => _absoluteAddress;

    public int RelativeAddress => _relativeAddress;

    public int FetchedValue => _fetchedData;

    public int Cycles => _cycles;

    public IInstruction CurrentInstruction => _currentInstruction ?? throw new InvalidOperationException("No instruction is currently being executed.");

    public int OpCode => _opCode;

    public bool Complete => _cycles == 0;

    private IBus bus => _bus ?? throw new InvalidOperationException("Bus not connected");

    public void ConnectBus(IBus bus)
    {
        _bus = bus;
    }

    public void DecreaseStackPointer()
    {
        _stackPointer--;
    }

    public void IncreaseCycles()
    {
        _cycles++;
    }

    public void IncreaseCycles(int cycles)
    {
        _cycles += cycles;
    }

    public void IncreaseStackPointer()
    {
        _stackPointer++;
    }

    public void SetAbsoluteAddress(int address)
    {
        _absoluteAddress = address;
    }

    public void SetAccumulatorRegister(int value)
    {
        _accumulatorRegister = value;
    }

    public void SetProgramCounter(int address)
    {
        _programCounter = address;
    }

    public void SetRelativeAddress(int offset)
    {
        _relativeAddress = offset;
    }

    public void SetStackPointer(int value)
    {
        _stackPointer = value;
    }

    public void SetStatus(int value)
    {
        _statusRegister = value;
    }

    public void SetXRegister(int value)
    {
        _xRegister = value;
    }

    public void SetYRegister(int value)
    {
        _yRegister = value;
    }

    public void StepProgramCounter(int offset)
    {
        _programCounter += offset;
    }

    public void StepProgramCounter()
    {
        _programCounter++;

    }

    public void Clock()
    {
        if (_cycles == 0)
        {
            _opCode = ReadMemory(_programCounter);
            _programCounter++;
            IInstruction instruction = Instruction.GetInstruction(_opCode);
            _currentInstruction = instruction;

            _cycles = instruction.Cycles;
            int additionalCycle1 = instruction.AddressingMode.Execute(this);
            int additionalCycle2 = instruction.Operation.Execute(this);

            _cycles += additionalCycle1 & additionalCycle2;
        }

        _cycles--;
    }

    public int Fetch()
    {
        var instruction = Instruction.GetInstruction(_opCode);

        if (instruction.AddressingMode != InstructionAddressingModes.Implied)
        {
            _fetchedData = ReadMemory(_absoluteAddress);
        }

        return _fetchedData;
    }

    public void Interrupt()
    {
        if (GetStatusFlag(Flags6502.I))
        {
            return;
        }

        WriteMemory(0x0100 + _stackPointer, (_programCounter >> 8).ToByte());
        _stackPointer--;
        WriteMemory(0x0100 + _stackPointer, _programCounter.ToByte());
        _stackPointer--;

        SetStatusFlag(Flags6502.B, false);
        SetStatusFlag(Flags6502.I, true);
        SetStatusFlag(Flags6502.U, true);
        WriteMemory(0x0100 + _stackPointer, _statusRegister.ToByte());
        _stackPointer--;

        _absoluteAddress = 0xFFFE;
        int lowByte = ReadMemory(_absoluteAddress);
        int highByte = ReadMemory(_absoluteAddress + 1);
        _programCounter = (highByte << 8) | lowByte;

        _cycles = 7;
    }

    public void LoadProgram(byte[] program, int startAddress)
    {
        throw new NotImplementedException();
    }

    public void NonMaskableInterrupt()
    {
        WriteMemory(0x0100 + _stackPointer, (_programCounter >> 8).ToByte());
        _stackPointer--;
        WriteMemory(0x0100 + _stackPointer, _programCounter.ToByte());
        _stackPointer--;

        SetStatusFlag(Flags6502.B, false);
        SetStatusFlag(Flags6502.I, true);
        SetStatusFlag(Flags6502.U, true);
        WriteMemory(0x0100 + _stackPointer, _statusRegister.ToByte());
        _stackPointer--;

        _absoluteAddress = 0xFFFA;
        int lowByte = ReadMemory(_absoluteAddress);
        int highByte = ReadMemory(_absoluteAddress + 1);
        _programCounter = (highByte << 8) | lowByte;

        _cycles = 8;
    }

    public void Reset()
    {
        _accumulatorRegister = 0x00;
        _xRegister = 0x00;
        _yRegister = 0x00;
        _stackPointer = 0xFD;
        _statusRegister = 0x00 | (int)Flags6502.U;
        _absoluteAddress = 0xFFFC;
        int lowByte = ReadMemory(_absoluteAddress);
        int highByte = ReadMemory(_absoluteAddress + 1);
        _programCounter = (highByte << 8) | lowByte;
        _absoluteAddress = 0x0000;
        _relativeAddress = 0x0000;
        _fetchedData = 0x00;
        _cycles = 8;
    }

    public bool GetStatusFlag(Flags6502 flag)
    {
        return (_statusRegister & (int)flag) > 0;
    }

    public void SetStatusFlag(Flags6502 flag, bool set)
    {
        if (set)
        {
            _statusRegister |= (int)flag;
        }
        else
        {
            _statusRegister &= ~(int)flag;
        }
    }

    public byte ReadMemory(int address, bool readOnly = false)
    {
        return bus.CpuRead(address, readOnly);
    }

    public byte ReadMemory()
    {
        return bus.CpuRead(_programCounter, true);
    }

    public void WriteMemory(int address, byte value)
    {
        bus.CpuWrite(address, value);
    }

    public void SetFetchedValue(int value)
    {
        _fetchedData = value;
    }
}

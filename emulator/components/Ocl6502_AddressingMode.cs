namespace emulator.components
{
    public partial class Ocl6502 : IAddressingModes
    {
        public int Absolute()
        {
            int lowByte = ReadMemory(programCounter);
            programCounter++;
            int highByte = ReadMemory(programCounter);
            programCounter++;

            absoluteAddress = (highByte << 8) | lowByte;

            return 0;

        }

        public int AbsoluteX()
        {
            int lowByte = ReadMemory(programCounter);
            programCounter++;
            int highByte = ReadMemory(programCounter);
            programCounter++;

            absoluteAddress = (highByte << 8) | lowByte;
            absoluteAddress += xRegister;

            if ((absoluteAddress & 0xFF00) != (highByte << 8))
            {
                return 1;
            }

            return 0;
        }

        public int AbsoluteY()
        {
            int lowByte = ReadMemory(programCounter);
            programCounter++;
            int highByte = ReadMemory(programCounter);
            programCounter++;

            absoluteAddress = (highByte << 8) | lowByte;
            absoluteAddress += yRegister;

            if ((absoluteAddress & 0xFF00) != (highByte << 8))
            {
                return 1;
            }
            
            return 0;
        }

        public int Immediate()
        {
            absoluteAddress = programCounter++;
            return 0;
        }

        public int Implied()
        {
            fetchedValue = accumulatorRegister;
            return 0;
        }

        public int Indirect()
        {
            int pointerLowByte = ReadMemory(programCounter);
            programCounter++;
            int pointerHighByte = ReadMemory(programCounter);
            programCounter++;

            int pointer = (pointerHighByte << 8) | pointerLowByte;

            if (pointerLowByte == 0x00FF)
            {
                absoluteAddress = (ReadMemory(pointer & 0xFF00) << 8) | ReadMemory(pointer);
            }
            else
            {
                absoluteAddress = (ReadMemory(pointer + 1) << 8) | ReadMemory(pointer);
            }

            return 0;
        }

        public int IndirectX()
        {
            int pointer = ReadMemory(programCounter);
            programCounter++;

            int lowByte = ReadMemory((pointer + xRegister) & 0x00FF);
            int highByte = ReadMemory((pointer + xRegister + 1) & 0x00FF);

            absoluteAddress = (highByte << 8) | lowByte;
            return 0;
        }

        public int IndirectY()
        {
            int pointer = ReadMemory(programCounter);
            programCounter++;

            int lowByte = ReadMemory(pointer & 0x00FF);
            int highByte = ReadMemory((pointer + 1) & 0x00FF);

            absoluteAddress = (highByte << 8) | lowByte;
            absoluteAddress += yRegister;

            if ((absoluteAddress & 0xFF00) != (highByte << 8))
            {
                return 1;
            }

            return 0;
           
        }

        public int Relative()
        {
            relativeAddress = ReadMemory(programCounter);
            programCounter++;

            if ((relativeAddress & 0x80) == 1)
            {
                relativeAddress |= 0xFF00; // Sign extend for negative offsets
            }

            return 0;
        }

        public int ZeroPage()
        {
            absoluteAddress = ReadMemory(programCounter);
            programCounter++;
            absoluteAddress &= 0x00FF; // Zero page addressing
            return 0;
        }

        public int ZeroPageX()
        {
            absoluteAddress = ReadMemory(programCounter) + xRegister;
            programCounter++;
            absoluteAddress &= 0x00FF;
            return 0;
        }

        public int ZeroPageY()
        {
            absoluteAddress = ReadMemory(programCounter) + yRegister;
            programCounter++;
            absoluteAddress &= 0x00FF;
            return 0;
        }
    }
}
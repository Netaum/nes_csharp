using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nes_csharp.components
{
    public interface IBus
    {
        // Define methods and properties that the Bus should implement
        int Read(int address);
        void Write(int address, int value, bool readOnly = false);
    }

    public class Bus : IBus
    {
        private const int MAX_ADDRESS = 0xFFFF; // Maximum address for the bus
        private const int MIN_ADDRESS = 0x0000; // Minimum address for the bus

        private int[] memory;

        public Bus(ICpu cpu)
        {
            memory = new int[MAX_ADDRESS];
            for(int i = 0; i < MAX_ADDRESS; i++)
            {
                memory[i] = 0x00;
            }

            cpu.ConnectBus(this); // Connect the CPU to this bus

        }

        // Implement the methods and properties defined in the IBus interface
        public int Read(int address)
        {
            if (address < MIN_ADDRESS || address > MAX_ADDRESS)
            {
                return 0;
            }
            
            return memory[address];
        }

        public void Write(int address, int value, bool readOnly = false)
        {
            if (address < MIN_ADDRESS || address > MAX_ADDRESS)
            {
                return;
            }

            memory[address] = value;

            if (!readOnly)
            {
                // Perform write operation
            }
        }
        
        // Additional methods and properties can be added as needed
    }
}
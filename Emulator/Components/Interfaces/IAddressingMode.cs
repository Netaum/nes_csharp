using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Emulator.Components.Interfaces
{
    public interface IAddressingMode
    {
        string Name { get; }
        int Execute(ICpu cpu);
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Interfaces
{
    public interface IInstruction
    {
        string Name { get; init; }
        int Cycles { get; init; }
        IAddressingMode AddressingMode { get; init; }
        IOpCode Operation { get; init; }
    }
}
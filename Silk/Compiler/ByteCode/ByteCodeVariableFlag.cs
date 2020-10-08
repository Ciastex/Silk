using System;

namespace Silk.Compiler.ByteCode
{
    [Flags]
    internal enum ByteCodeVariableFlag
    {
        None = 0x00000000,
        Global = 0x10000000,
        Local = 0x20000000,
        Parameter = 0x40000000,
        All = Global | Local | Parameter,
    }
}
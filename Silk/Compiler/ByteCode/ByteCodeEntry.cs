namespace Silk.Compiler.ByteCode
{
    /// <summary>
    /// Represents a bytecode entry. Additional properties used primarily for
    /// creating bytecode log file, and for extracting line numbers for
    /// runtime.
    /// </summary>
    internal class ByteCodeEntry
    {
        public int Value { get; set; }
        public bool IsBytecode { get; set; }
        public int Line { get; set; }

        public ByteCodeEntry(OpCode bytecode, int line)
        {
            Value = (int)bytecode;
            IsBytecode = true;
            Line = line;
        }

        public ByteCodeEntry(int value, int line)
        {
            Value = value;
            IsBytecode = false;
            Line = line;
        }

        public override string ToString()
        {
            if (IsBytecode)
            {
                return string.Format("ByteCode.{0} ({1})", (OpCode)Value, Value);
            }
            else
            {
                string s = string.Format("{0} (0x{0:x08})", Value);
                ByteCodeVariableFlag flag = (ByteCodeVariableFlag)Value & ByteCodeVariableFlag.All;
                if (flag != ByteCodeVariableFlag.None)
                    s += $" : {flag}[{Value & ~(int)ByteCodeVariableFlag.All}]";
                return s;
            }
        }
    }

}
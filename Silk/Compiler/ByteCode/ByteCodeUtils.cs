namespace Silk.Compiler.ByteCode
{
    internal static class ByteCodeUtils
    {
        public const int InvalidIP = -1;

        public static bool IsGlobalVariable(int value) =>
            ((ByteCodeVariableFlag)value).HasFlag(ByteCodeVariableFlag.Global);
        public static bool IsLocalVariable(int value) =>
            ((ByteCodeVariableFlag)value).HasFlag(ByteCodeVariableFlag.Local);
        public static bool IsFunctionParameterVariable(int value) =>
            ((ByteCodeVariableFlag)value).HasFlag(ByteCodeVariableFlag.Parameter);
        public static int GetVariableIndex(int value) => (value & ~(int)ByteCodeVariableFlag.All);
    }
}
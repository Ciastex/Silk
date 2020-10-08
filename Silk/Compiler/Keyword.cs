namespace Silk.Compiler
{
    /// <summary>
    /// Enum of supported keywords.
    /// </summary>
    internal enum Keyword
    {
        NullKeyword,    // Not a keyword
        Var,
        GoTo,
        Return,
        If,
        Else,
        While,
        For,
        // Keywords part of other statements/expressions
        And,
        Or,
        Xor,
        Not,
        To,
        Step,
    }
}